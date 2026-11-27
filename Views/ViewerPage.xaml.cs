using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using PdfReader.Core;
using PdfReader.Localization;
using PdfReader.Services;
using Windows.Data.Pdf;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;

namespace PdfReader.Views;

/// <summary>
/// Continuous-scroll PDF viewer built on Windows.Data.Pdf. Every page has a correctly sized placeholder
/// so scrolling and page jumps are exact; bitmaps are rendered only for pages near the viewport.
/// One instance per open tab, hosted by MainWindow; <see cref="DocumentTabService"/> drives its lifecycle.
/// </summary>
public sealed partial class ViewerPage : Page
{
    private const double PagePadding = 24;
    private const double PageGap = 12;
    private const double MinZoom = 0.1;
    private const double MaxZoom = 5;
    private const double MaxBitmapWidth = 3600;
    private const double MaxBitmapPixels = 16_000_000;
    private const double MaxBitmapHeight = 12_000;
    private const int KeepAround = 4;
    private const int PendingTimeoutMs = 250;

    private const int WrongPasswordHResult = unchecked((int)0x8007052B);

    private readonly IRecentFilesService _recent = App.Services.GetRequiredService<IRecentFilesService>();
    private readonly ILogger<ViewerPage> _logger = App.Services.GetRequiredService<ILogger<ViewerPage>>();
    private readonly Microsoft.UI.Dispatching.DispatcherQueueTimer _saveTimer;
    private readonly SemaphoreSlim _renderGate = new(2);

    private PdfDocument? _doc;
    private string _path = string.Empty;
    private Size[] _sizes = [];
    private double[] _tops = [];
    private Border?[] _frames = [];
    private int[] _renderedWidth = [];
    private int[] _generation = [];
    private readonly HashSet<int> _rendered = [];
    private readonly List<int> _evict = [];
    private readonly Stack<Border> _framePool = new();
    private double _zoom = 1;
    private double _total; // document height in DIPs
    private double _windowStart; // document offset of the top of the scrollable canvas (see ScrollWindow)
    private double? _pendingOffset; // target of a ChangeView that has not landed yet
    private long _pendingSince;
    private int _shownFrom;
    private int _shownTo = -1;
    private double _maxPageWidth = 1;
    private bool _fitWidth = true;
    private int _currentPage;
    private bool _updatingBox;
    private bool _updatingBar;
    private string? _errorKey;
    private bool _suspended; // the tab is hidden
    private bool _closed; // the tab was closed; nothing may touch the document again
    private ViewState? _resumeState; // position to restore when the tab is shown again

    /// <summary>Scroll position expressed in document DIPs.</summary>
    private double RealOffset => _pendingOffset ?? _windowStart + Scroller.VerticalOffset;

    public ViewerPage()
    {
        InitializeComponent();

        _saveTimer = DispatcherQueue.CreateTimer();
        _saveTimer.Interval = TimeSpan.FromSeconds(1);
        _saveTimer.IsRepeating = false;
        _saveTimer.Tick += (_, _) => SaveProgress();

        AddAccelerator(VirtualKey.Add, () => StepZoom(1.2));
        AddAccelerator((VirtualKey)187, () => StepZoom(1.2)); // Ctrl + '=' / '+'
        AddAccelerator(VirtualKey.Subtract, () => StepZoom(1 / 1.2));
        AddAccelerator((VirtualKey)189, () => StepZoom(1 / 1.2)); // Ctrl + '-'
        AddAccelerator(VirtualKey.Number0, ApplyFitWidth);
        // Ctrl+O (open) and F11 (full screen) are window-wide shortcuts handled by MainWindow.

        Pages.PointerWheelChanged += OnPagesWheel;
        KeyDown += OnKeyDown;
        // The scroller would handle Home/End/PageUp/PageDown itself, but it only knows the current window.
        Scroller.PreviewKeyDown += OnKeyDown;

        InitializeSelection();
        ApplyPageListSetting();
    }

    private void AddAccelerator(VirtualKey key, Action action)
    {
        var accelerator = new KeyboardAccelerator { Key = key, Modifiers = VirtualKeyModifiers.Control };
        accelerator.Invoked += (_, e) =>
        {
            action();
            e.Handled = true;
        };
        KeyboardAccelerators.Add(accelerator);
    }

    // ---------------------------------------------------------------- lifecycle (driven by the tab service)

    /// <summary>The reading position or zoom changed; raised at most about once a second.</summary>
    public event EventHandler? StateChanged;

    /// <summary>The document cannot be shown (e.g. the password prompt was cancelled), so its tab should close.</summary>
    public event EventHandler? CloseRequested;

    /// <summary>Starts loading a document, at <paramref name="view"/> or else where it was last read.</summary>
    public void Open(string path, ViewState? view)
    {
        _path = path;
        TitleText.Text = System.IO.Path.GetFileName(_path);
        Loc.Service.LanguageChanged += OnLanguageChanged;
        _ = LoadAsync(view);
    }

    /// <summary>The tab was hidden: remember the position and free the page bitmaps, but keep the document open.</summary>
    public void Suspend()
    {
        if (_suspended || _closed) return;
        _resumeState = CaptureState();
        _suspended = true;
        _saveTimer.Stop();
        SaveProgress();
        EndDrag();
        ReleaseAllFrames();
    }

    /// <summary>The tab is shown again: restore the position once the viewer is back in the window.</summary>
    public void Resume()
    {
        if (!_suspended || _closed) return;
        _suspended = false;
        ApplyPageListSetting();
        var state = _resumeState;
        _resumeState = null;
        RunWhenLoaded(() =>
        {
            if (state is not null) ApplyView(state);
            Scroller.Focus(FocusState.Programmatic); // so the keyboard scrolls the document right away
        });
    }

    /// <summary>The tab was closed: save the position and release everything the document holds.</summary>
    public void Close()
    {
        if (_closed) return;
        _saveTimer.Stop();
        SaveProgress();
        _closed = true;
        Loc.Service.LanguageChanged -= OnLanguageChanged;
        ReleaseAllFrames();
        Pages.Children.Clear();
        _framePool.Clear();
        _doc = null;
        ClearPageList();
        CloseTextLayer();
    }

    /// <summary>The current position and zoom; null until the document is loaded.</summary>
    public ViewState? CaptureState()
    {
        if (_doc is null || _tops.Length == 0) return null;
        if (_suspended) return _resumeState;
        var top = RealOffset;
        var page = PageAt(top);
        var fraction = Math.Max(0, (top - _tops[page]) / Math.Max(1, PageHeight(page)));
        return new ViewState(page, fraction, _zoom, _fitWidth, Scroller.HorizontalOffset);
    }

    private void ApplyView(ViewState view)
    {
        if (_doc is null || _suspended || _closed) return;
        var page = Math.Clamp(view.Page, 0, _tops.Length - 1);
        _fitWidth = view.FitWidth;
        _zoom = Math.Clamp(view.FitWidth ? ViewportWidth / _maxPageWidth : view.Zoom, MinZoom, MaxZoom);
        LayoutPages();
        Pages.UpdateLayout();

        // At the very top of a page, keep the gap above it in view, as page jumps do.
        var offset = view.PageFraction > 0 ? _tops[page] + view.PageFraction * PageHeight(page) : _tops[page] - PageGap;
        ScrollToOffset(offset, view.HorizontalOffset);
        SetCurrentPage(PageAt(RealOffset + Scroller.ViewportHeight / 3));
    }

    /// <summary>Runs an action after the viewer is in the window and has been laid out.</summary>
    private void RunWhenLoaded(Action action)
    {
        void Run() => DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () => action());
        if (IsLoaded) { Run(); return; }
        void OnLoaded(object s, RoutedEventArgs e) { Loaded -= OnLoaded; Run(); }
        Loaded += OnLoaded;
    }

    private void ReleaseAllFrames()
    {
        for (var i = _shownFrom; i <= _shownTo; i++) ReleaseFrame(i);
        _shownFrom = 0;
        _shownTo = -1;
    }

    // ---------------------------------------------------------------- loading

    private async Task LoadAsync(ViewState? view)
    {
        BusyRing.IsActive = true;
        try
        {
            await WaitUntilLoadedAsync();
            var file = await StorageFile.GetFileFromPathAsync(_path);
            var (doc, password) = await LoadDocumentAsync(file);
            if (_closed) return;
            if (doc is null)
            {
                CloseRequested?.Invoke(this, EventArgs.Empty);
                return;
            }

            var count = (int)doc.PageCount;
            if (count == 0) throw new InvalidDataException("The document has no pages.");

            var sizes = await Task.Run(() =>
            {
                var result = new Size[count];
                for (var i = 0; i < count; i++)
                {
                    using var page = doc.GetPage((uint)i);
                    result[i] = page.Size;
                }
                return result;
            });
            if (_closed) return;

            _doc = doc;
            OpenTextLayer(password);
            _sizes = sizes;
            _maxPageWidth = Math.Max(1, sizes.Max(s => s.Width));
            _tops = new double[count];
            _renderedWidth = new int[count];
            _generation = new int[count];
            _frames = new Border?[count];
            FillPageList();

            PageBox.Maximum = count;
            PageBox.Width = Math.Max(64, 40 + 10 * count.ToString().Length);
            PageCountText.Text = $"/ {count}";

            var info = await _recent.GetAsync(_path);
            if (_closed) return;
            var start = view ?? new ViewState(Page: Math.Clamp(info?.LastPage ?? 0, 0, count - 1));

            if (_suspended)
            {
                _resumeState = start; // the tab was hidden while loading; Resume applies it
            }
            else
            {
                ApplyView(start);
                UpdateVisible();
                Scroller.Focus(FocusState.Programmatic);
            }

            await _recent.UpdateProgressAsync(_path, Math.Clamp(start.Page, 0, count - 1), count);
        }
        catch (Exception ex) when (_closed)
        {
            _logger.LogDebug(ex, "Load of {Path} ended after its tab closed", _path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open {Path}", _path);
            _errorKey = ex is FileNotFoundException or DirectoryNotFoundException or ArgumentException or UnauthorizedAccessException
                ? "viewer.error.access"
                : "viewer.error.invalid";
            ErrorBar.Message = Loc.Get(_errorKey);
            ErrorBar.IsOpen = true;
            _ = _recent.RemoveAsync(_path); // do not keep unopenable files in the library
        }
        finally
        {
            BusyRing.IsActive = false;
        }
    }

    /// <summary>
    /// Loads the document, asking for a password when it is encrypted, and returns the password that opened it
    /// (the text layer needs it too). The document is null if the user cancels.
    /// </summary>
    private async Task<(PdfDocument? Doc, string? Password)> LoadDocumentAsync(StorageFile file)
    {
        string? password = null;
        while (true)
        {
            try
            {
                var doc = password is null
                    ? await PdfDocument.LoadFromFileAsync(file)
                    : await PdfDocument.LoadFromFileAsync(file, password);
                return (doc, password);
            }
            catch (Exception ex) when (ex.HResult == WrongPasswordHResult)
            {
                password = await AskPasswordAsync(retry: password is not null);
                if (password is null) return (null, null);
            }
        }
    }

    private async Task<string?> AskPasswordAsync(bool retry)
    {
        await WaitUntilLoadedAsync();
        var box = new PasswordBox { PlaceholderText = Loc.Get("pw.placeholder") };
        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = Loc.Get(retry ? "pw.retryTitle" : "pw.title"),
            Content = box,
            PrimaryButtonText = Loc.Get("pw.open"),
            CloseButtonText = Loc.Get("pw.cancel"),
            DefaultButton = ContentDialogButton.Primary,
        };
        return await dialog.ShowAsync() == ContentDialogResult.Primary ? box.Password : null;
    }

    /// <summary>On fast launches the page can start loading before it is attached to the window (no XamlRoot yet).</summary>
    private async Task WaitUntilLoadedAsync()
    {
        if (XamlRoot is not null) return;
        var tcs = new TaskCompletionSource();
        void OnLoaded(object s, RoutedEventArgs e) { Loaded -= OnLoaded; tcs.TrySetResult(); }
        Loaded += OnLoaded;
        if (XamlRoot is not null) { Loaded -= OnLoaded; return; }
        await tcs.Task;
    }

    // ---------------------------------------------------------------- layout

    /// <summary>Only pages near the viewport own a frame; frames are recycled, so the element count stays small.</summary>
    private Border GetFrame(int i)
    {
        if (_frames[i] is { } existing) return existing;
        if (!_framePool.TryPop(out var frame))
        {
            frame = new Border
            {
                Background = new SolidColorBrush(Microsoft.UI.Colors.White),
                BorderBrush = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"],
                BorderThickness = new Thickness(1),
                Child = new Grid
                {
                    IsHitTestVisible = false,
                    Children =
                    {
                        new Image { Stretch = Stretch.Fill },
                        new Viewbox
                        {
                            Stretch = Stretch.Fill,
                            Child = new Canvas { Width = HighlightSpace, Height = HighlightSpace },
                        },
                    },
                },
                Visibility = Visibility.Collapsed,
            };
            Pages.Children.Add(frame);
        }
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(frame, Loc.Format("viewer.page", i + 1));
        _frames[i] = frame;
        DrawHighlights(i);
        return frame;
    }

    private static Image FrameImage(Border frame) => (Image)((Grid)frame.Child).Children[0];

    private static Canvas FrameHighlights(Border frame) => (Canvas)((Viewbox)((Grid)frame.Child).Children[1]).Child;

    private double PageLeft(int page) => Math.Max(PagePadding, (Pages.Width - _sizes[page].Width * _zoom) / 2);

    private void ReleaseFrame(int i)
    {
        if (_frames[i] is not { } frame) return;
        _frames[i] = null;
        _generation[i]++; // drops a render still in flight for this page
        _renderedWidth[i] = 0;
        _rendered.Remove(i);
        FrameImage(frame).Source = null;
        FrameHighlights(frame).Children.Clear();
        frame.Visibility = Visibility.Collapsed;
        _framePool.Push(frame);
    }

    private void LayoutPages()
    {
        var y = PagePadding;
        for (var i = 0; i < _frames.Length; i++)
        {
            _tops[i] = y;
            y += _sizes[i].Height * _zoom + PageGap;
        }

        _total = y - PageGap + PagePadding;
        Pages.Height = Math.Min(_total, ScrollWindow.Extent);
        ApplyCanvasWidth();

        // The built-in scroll bar only knows the window, so long documents get one that spans the whole document.
        var windowed = _total > ScrollWindow.Extent;
        Scroller.VerticalScrollBarVisibility = windowed ? ScrollBarVisibility.Hidden : ScrollBarVisibility.Auto;
        DocumentBar.Visibility = windowed ? Visibility.Visible : Visibility.Collapsed;
        ZoomButton.Content = $"{_zoom * 100:0}%";
    }

    private void ApplyCanvasWidth() =>
        Pages.Width = Math.Max(_maxPageWidth * _zoom + 2 * PagePadding, Scroller.ViewportWidth);

    /// <summary>Places the pages in [from, to] at their on-screen positions and hides the rest.</summary>
    private void PositionPages(int from, int to)
    {
        for (var i = _shownFrom; i <= _shownTo; i++)
            if (i < from || i > to) ReleaseFrame(i);

        for (var i = from; i <= to; i++)
        {
            var frame = GetFrame(i);
            frame.Width = _sizes[i].Width * _zoom;
            frame.Height = _sizes[i].Height * _zoom;
            Canvas.SetTop(frame, _tops[i] - _windowStart);
            Canvas.SetLeft(frame, PageLeft(i));
            frame.Visibility = Visibility.Visible;
        }
        _shownFrom = from;
        _shownTo = to;
    }

    private int PageAt(double y)
    {
        if (_tops.Length == 0) return 0;
        var index = Array.BinarySearch(_tops, y);
        if (index < 0) index = ~index - 1;
        return Math.Clamp(index, 0, _tops.Length - 1);
    }

    private void SetZoom(double zoom)
    {
        if (_doc is null) return;
        zoom = Math.Clamp(zoom, MinZoom, MaxZoom);

        // Keep the page under the top of the viewport (and the position inside it) stable.
        var page = PageAt(RealOffset);
        var fraction = (RealOffset - _tops[page]) / Math.Max(1, _sizes[page].Height * _zoom);

        _zoom = zoom;
        LayoutPages();
        Pages.UpdateLayout();
        ScrollToOffset(_tops[page] + fraction * _sizes[page].Height * _zoom);
        RestartSaveTimer();
    }

    private double PageHeight(int page) => _sizes[page].Height * _zoom;

    private void StepZoom(double factor)
    {
        _fitWidth = false;
        SetZoom(_zoom * factor);
    }

    private double ViewportWidth => Math.Max(100, Scroller.ActualWidth - 2 * PagePadding - 20);

    private void ApplyFitWidth()
    {
        _fitWidth = true;
        SetZoom(ViewportWidth / _maxPageWidth);
    }

    private void ScrollToPage(int page)
    {
        if (_doc is null) return;
        page = Math.Clamp(page, 0, _tops.Length - 1);
        Pages.UpdateLayout();
        ScrollToOffset(_tops[page] - PageGap);
    }

    /// <summary>Scrolls to a document offset, sliding the scrollable window first when it needs to move.</summary>
    private void ScrollToOffset(double offset, double? horizontal = null)
    {
        offset = Math.Clamp(offset, 0, Math.Max(0, _total - Scroller.ViewportHeight));
        var start = ScrollWindow.Place(_windowStart, offset, _total);
        var slid = start != _windowStart;
        _windowStart = start;

        // ChangeView lands on a later tick; until then VerticalOffset is stale, so remember where we are going.
        _pendingOffset = offset;
        _pendingSince = Environment.TickCount64;
        Scroller.ChangeView(horizontal, offset - start, null, true);
        if (HasArrived(offset)) _pendingOffset = null;

        // After a slide the pages must move in the same tick as the view (OnViewChanged), or a blank frame shows.
        if (!slid || _pendingOffset is null) UpdateVisible();
    }

    private bool HasArrived(double offset) => Math.Abs(Scroller.VerticalOffset - (offset - _windowStart)) < 1;

    private void SyncDocumentBar(double top)
    {
        if (DocumentBar.Visibility != Visibility.Visible) return;
        var viewport = Scroller.ViewportHeight;
        _updatingBar = true;
        DocumentBar.ViewportSize = viewport;
        DocumentBar.LargeChange = viewport * 0.9;
        DocumentBar.Maximum = Math.Max(0, _total - viewport);
        DocumentBar.Value = top;
        _updatingBar = false;
    }

    // ---------------------------------------------------------------- rendering

    private void UpdateVisible()
    {
        if (_doc is null || _suspended) return;

        var top = RealOffset;
        var first = PageAt(top);
        var last = PageAt(top + Scroller.ViewportHeight);
        PositionPages(Math.Max(0, first - KeepAround), Math.Min(_frames.Length - 1, last + KeepAround));

        // Zoomed in, a single page is huge: render only what is on screen and keep fewer pages around.
        var ahead = _zoom > 2 ? 0 : 1;
        var keep = _zoom > 2 ? 1 : KeepAround;
        for (var i = Math.Max(0, first - ahead); i <= Math.Min(_frames.Length - 1, last + ahead); i++)
        {
            var target = TargetPixelWidth(i);
            if (_renderedWidth[i] != target) _ = RenderPageAsync(i, target);
        }

        _evict.Clear();
        foreach (var i in _rendered)
            if (i < first - keep || i > last + keep) _evict.Add(i);
        foreach (var i in _evict)
        {
            _generation[i]++;
            _renderedWidth[i] = 0;
            _rendered.Remove(i);
            FrameImage(_frames[i]!).Source = null;
        }

        var current = PageAt(top + Scroller.ViewportHeight / 3);
        if (current != _currentPage) SetCurrentPage(current);
        SyncDocumentBar(top);
    }

    private int TargetPixelWidth(int page)
    {
        var scale = XamlRoot?.RasterizationScale ?? 1;
        var size = _sizes[page];
        var aspect = size.Height / size.Width;
        var width = Math.Ceiling(size.Width * _zoom * scale);
        width = Math.Min(width, Math.Min(MaxBitmapWidth, Math.Min(MaxBitmapHeight / aspect, Math.Sqrt(MaxBitmapPixels / aspect))));
        return (int)Math.Max(1, Math.Floor(width));
    }

    private async Task RenderPageAsync(int index, int pixelWidth)
    {
        var doc = _doc;
        if (doc is null) return;

        var generation = ++_generation[index];
        _renderedWidth[index] = pixelWidth;
        _rendered.Add(index);
        var entered = false;
        try
        {
            await _renderGate.WaitAsync();
            entered = true;
            if (generation != _generation[index] || doc != _doc) return; // superseded while queued

            var size = _sizes[index];
            var pixelHeight = (uint)Math.Max(1, Math.Round(pixelWidth * size.Height / size.Width));

            using var stream = new InMemoryRandomAccessStream();
            using (var page = doc.GetPage((uint)index))
            {
                await page.RenderToStreamAsync(stream, new PdfPageRenderOptions
                {
                    DestinationWidth = (uint)pixelWidth,
                    DestinationHeight = pixelHeight,
                });
            }

            if (generation != _generation[index] || doc != _doc) return;

            var bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(stream);
            if (generation != _generation[index] || doc != _doc) return;

            FrameImage(GetFrame(index)).Source = bitmap;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to render page {Page}", index + 1);
            if (generation == _generation[index]) { _renderedWidth[index] = 0; _rendered.Remove(index); }
        }
        finally
        {
            if (entered) _renderGate.Release();
        }
    }

    // ---------------------------------------------------------------- current page

    private void SetCurrentPage(int page)
    {
        _currentPage = page;
        _updatingBox = true;
        PageBox.Value = page + 1;
        _updatingBox = false;
        SyncPageList();
        RestartSaveTimer();
    }

    private void RestartSaveTimer()
    {
        if (_closed) return;
        _saveTimer.Stop();
        _saveTimer.Start();
    }

    /// <summary>Records the page in the library and lets the tab session save the full position.</summary>
    private void SaveProgress()
    {
        if (_doc is null || _closed) return;
        _ = _recent.UpdateProgressAsync(_path, _currentPage, (int)_doc.PageCount);
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    // ---------------------------------------------------------------- events

    private void OnViewChanged(object? sender, ScrollViewerViewChangedEventArgs e)
    {
        if (_suspended) return;
        if (_pendingOffset is { } pending)
        {
            // The time limit covers a target the scroller clamped and so will never reach.
            if (!HasArrived(pending) && Environment.TickCount64 - _pendingSince < PendingTimeoutMs) return;
            _pendingOffset = null;
        }

        // Slide only once scrolling settles: moving the view would cut off a running pan or wheel animation.
        if (!e.IsIntermediate && _doc is not null && ScrollWindow.Place(_windowStart, RealOffset, _total) != _windowStart)
            ScrollToOffset(RealOffset);
        else
            UpdateVisible();
    }

    private void OnDocumentBarChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (!_updatingBar && _doc is not null) ScrollToOffset(e.NewValue);
    }

    private void OnScrollerSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_doc is null || _suspended) return;
        ApplyCanvasWidth();
        if (_fitWidth) SetZoom(ViewportWidth / _maxPageWidth);
        else UpdateVisible();
    }

    private void OnPageBoxChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (_updatingBox || _doc is null) return;
        if (double.IsNaN(args.NewValue))
        {
            _updatingBox = true;
            sender.Value = _currentPage + 1;
            _updatingBox = false;
            return;
        }
        ScrollToPage((int)args.NewValue - 1);
        Scroller.Focus(FocusState.Programmatic);
    }

    private void OnPagesWheel(object sender, PointerRoutedEventArgs e)
    {
        if (!e.KeyModifiers.HasFlag(VirtualKeyModifiers.Control)) return;
        var delta = e.GetCurrentPoint(Scroller).Properties.MouseWheelDelta;
        StepZoom(delta > 0 ? 1.1 : 1 / 1.1);
        e.Handled = true;
    }

    private void OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (_doc is null || e.OriginalSource is TextBox or PasswordBox or NumberBox) return;
        if (Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control)
                .HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down)) return;

        var page = Scroller.ViewportHeight * 0.9;
        switch (e.Key)
        {
            case VirtualKey.Home: ScrollToPage(0); break;
            case VirtualKey.End: ScrollToPage(_frames.Length - 1); break;
            case VirtualKey.PageDown or VirtualKey.Space: ScrollBy(page); break;
            case VirtualKey.PageUp: ScrollBy(-page); break;
            case VirtualKey.Down: ScrollBy(60); break;
            case VirtualKey.Up: ScrollBy(-60); break;
            case VirtualKey.Right: Scroller.ChangeView(Scroller.HorizontalOffset + 60, null, null, true); break;
            case VirtualKey.Left: Scroller.ChangeView(Scroller.HorizontalOffset - 60, null, null, true); break;
            default: return;
        }
        e.Handled = true;
    }

    private void ScrollBy(double delta) => ScrollToOffset(RealOffset + delta);

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        if (_errorKey is not null) ErrorBar.Message = Loc.Get(_errorKey);
        for (var i = 0; i < _frames.Length; i++)
            if (_frames[i] is { } frame)
                Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(frame, Loc.Format("viewer.page", i + 1));
    }

    private void OnBack(object sender, RoutedEventArgs e) =>
        App.Services.GetRequiredService<INavigationService>().GoBack();

    private void OnZoomIn(object sender, RoutedEventArgs e) => StepZoom(1.2);
    private void OnZoomOut(object sender, RoutedEventArgs e) => StepZoom(1 / 1.2);
    private void OnFitWidth(object sender, RoutedEventArgs e) => ApplyFitWidth();

    private void OnFitPage(object sender, RoutedEventArgs e)
    {
        if (_doc is null) return;
        _fitWidth = false;
        var size = _sizes[_currentPage];
        var availableHeight = Math.Max(100, Scroller.ActualHeight - 2 * PagePadding);
        SetZoom(Math.Min(ViewportWidth / size.Width, availableHeight / size.Height));
        ScrollToPage(_currentPage);
    }

    private void OnZoomPreset(object sender, RoutedEventArgs e)
    {
        _fitWidth = false;
        SetZoom(double.Parse((string)((FrameworkElement)sender).Tag, System.Globalization.CultureInfo.InvariantCulture));
    }
}
