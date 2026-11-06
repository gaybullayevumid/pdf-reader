using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
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

    // XAML cannot scroll past ~1M DIPs, so very long documents are mapped onto a shorter scroll range.
    private const double MaxExtent = 600_000;
    private const int WrongPasswordHResult = unchecked((int)0x8007052B);

    private readonly IRecentFilesService _recent = App.Services.GetRequiredService<IRecentFilesService>();
    private readonly ILogger<ViewerPage> _logger = App.Services.GetRequiredService<ILogger<ViewerPage>>();
    private readonly Microsoft.UI.Dispatching.DispatcherQueueTimer _saveTimer;
    private readonly SemaphoreSlim _renderGate = new(2);

    private PdfDocument? _doc;
    private string _path = string.Empty;
    private Size[] _sizes = [];
    private double[] _tops = [];
    private Border[] _frames = [];
    private int[] _renderedWidth = [];
    private int[] _generation = [];
    private double _zoom = 1;
    private double _scale = 1; // virtual scroll units per document DIP (<= 1)
    private int _shownFrom;
    private int _shownTo = -1;
    private double _maxPageWidth = 1;
    private bool _fitWidth = true;
    private int _currentPage;
    private bool _updatingBox;
    private string? _errorKey;

    /// <summary>Scroll position expressed in document DIPs.</summary>
    private double RealOffset => Scroller.VerticalOffset / _scale;

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
        AddAccelerator(VirtualKey.O, () => _ = App.Services.GetRequiredService<IDocumentOpener>().PickAndOpenAsync());

        Pages.PointerWheelChanged += OnPagesWheel;
        KeyDown += OnKeyDown;
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

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _path = e.Parameter as string ?? string.Empty;
        TitleText.Text = System.IO.Path.GetFileName(_path);
        Loc.Service.LanguageChanged += OnLanguageChanged;
        await LoadAsync();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        Loc.Service.LanguageChanged -= OnLanguageChanged;
        _saveTimer.Stop();
        SaveProgress();
        _doc = null;
    }

    // ---------------------------------------------------------------- loading

    private async Task LoadAsync()
    {
        BusyRing.IsActive = true;
        try
        {
            await WaitUntilLoadedAsync();
            var file = await StorageFile.GetFileFromPathAsync(_path);
            var doc = await LoadDocumentAsync(file);
            if (doc is null)
            {
                App.Services.GetRequiredService<INavigationService>().GoBack();
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

            _doc = doc;
            _sizes = sizes;
            _maxPageWidth = Math.Max(1, sizes.Max(s => s.Width));
            _tops = new double[count];
            _renderedWidth = new int[count];
            _generation = new int[count];
            _frames = new Border[count];

            for (var i = 0; i < count; i++)
            {
                _frames[i] = new Border
                {
                    Background = new SolidColorBrush(Microsoft.UI.Colors.White),
                    BorderBrush = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"],
                    BorderThickness = new Thickness(1),
                    Child = new Image { Stretch = Stretch.Fill, IsHitTestVisible = false },
                    Visibility = Visibility.Collapsed,
                };
                Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(_frames[i], Loc.Format("viewer.page", i + 1));
                Pages.Children.Add(_frames[i]);
            }

            PageBox.Maximum = count;
            PageCountText.Text = $"/ {count}";

            var info = await _recent.GetAsync(_path);
            var startPage = Math.Clamp(info?.LastPage ?? 0, 0, count - 1);

            ApplyFitWidth();
            ScrollToPage(startPage);
            SetCurrentPage(startPage);
            UpdateVisible();
            Scroller.Focus(FocusState.Programmatic);

            await _recent.UpdateProgressAsync(_path, startPage, count);
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

    /// <summary>Loads the document, asking for a password when it is encrypted. Returns null if the user cancels.</summary>
    private async Task<PdfDocument?> LoadDocumentAsync(StorageFile file)
    {
        string? password = null;
        while (true)
        {
            try
            {
                return password is null
                    ? await PdfDocument.LoadFromFileAsync(file)
                    : await PdfDocument.LoadFromFileAsync(file, password);
            }
            catch (Exception ex) when (ex.HResult == WrongPasswordHResult)
            {
                password = await AskPasswordAsync(retry: password is not null);
                if (password is null) return null;
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

    private void LayoutPages()
    {
        var y = PagePadding;
        for (var i = 0; i < _frames.Length; i++)
        {
            _tops[i] = y;
            _frames[i].Width = _sizes[i].Width * _zoom;
            _frames[i].Height = _sizes[i].Height * _zoom;
            y += _frames[i].Height + PageGap;
        }

        var total = y - PageGap + PagePadding;
        _scale = total > MaxExtent ? MaxExtent / total : 1;
        Pages.Height = total * _scale;
        ApplyCanvasWidth();

        for (var i = _shownFrom; i <= _shownTo; i++) _frames[i].Visibility = Visibility.Collapsed;
        _shownTo = -1;
        ZoomButton.Content = $"{_zoom * 100:0}%";
    }

    private void ApplyCanvasWidth() =>
        Pages.Width = Math.Max(_maxPageWidth * _zoom + 2 * PagePadding, Scroller.ViewportWidth);

    /// <summary>Places the pages in [from, to] at their on-screen positions and hides the rest.</summary>
    private void PositionPages(int from, int to)
    {
        for (var i = _shownFrom; i <= _shownTo; i++)
            if (i < from || i > to) _frames[i].Visibility = Visibility.Collapsed;

        var shift = Scroller.VerticalOffset - RealOffset; // canvas offset that cancels scroll compression
        for (var i = from; i <= to; i++)
        {
            Canvas.SetTop(_frames[i], _tops[i] + shift);
            Canvas.SetLeft(_frames[i], Math.Max(PagePadding, (Pages.Width - _frames[i].Width) / 2));
            _frames[i].Visibility = Visibility.Visible;
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
        Scroller.ChangeView(null, (_tops[page] + fraction * _sizes[page].Height * _zoom) * _scale, null, true);
        UpdateVisible();
    }

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
        Scroller.ChangeView(null, Math.Max(0, _tops[page] - PageGap) * _scale, null, true);
    }

    // ---------------------------------------------------------------- rendering

    private void UpdateVisible()
    {
        if (_doc is null) return;

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

        for (var i = 0; i < _frames.Length; i++)
        {
            if (_renderedWidth[i] == 0 || (i >= first - keep && i <= last + keep)) continue;
            _generation[i]++;
            _renderedWidth[i] = 0;
            ((Image)_frames[i].Child).Source = null;
        }

        var current = PageAt(top + Scroller.ViewportHeight / 3);
        if (current != _currentPage) SetCurrentPage(current);
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

            ((Image)_frames[index].Child).Source = bitmap;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to render page {Page}", index + 1);
            if (generation == _generation[index]) _renderedWidth[index] = 0;
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
        _saveTimer.Stop();
        _saveTimer.Start();
    }

    private void SaveProgress()
    {
        if (_doc is null) return;
        _ = _recent.UpdateProgressAsync(_path, _currentPage, (int)_doc.PageCount);
    }

    // ---------------------------------------------------------------- events

    private void OnViewChanged(object? sender, ScrollViewerViewChangedEventArgs e) => UpdateVisible();

    private void OnScrollerSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_doc is null) return;
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

    private void ScrollBy(double delta) => Scroller.ChangeView(null, Scroller.VerticalOffset + delta * _scale, null, true);

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        if (_errorKey is not null) ErrorBar.Message = Loc.Get(_errorKey);
        for (var i = 0; i < _frames.Length; i++)
            Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(_frames[i], Loc.Format("viewer.page", i + 1));
    }

    private void OnOpenPdf(object sender, RoutedEventArgs e) =>
        _ = App.Services.GetRequiredService<IDocumentOpener>().PickAndOpenAsync();

    private void OnToggleFullScreen(object sender, RoutedEventArgs e) =>
        App.Services.GetRequiredService<IWindowStateService>().ToggleFullScreen();

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
