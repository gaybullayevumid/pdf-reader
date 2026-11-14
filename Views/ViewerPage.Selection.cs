using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using PdfReader.Core;
using PdfReader.Localization;
using PdfReader.Services;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.System;

namespace PdfReader.Views;

/// <summary>
/// Text selection: drag with the mouse or pen to select (Shift+click extends, double-click picks a word),
/// Ctrl+C or the context menu copies, Ctrl+A selects everything. Touch keeps panning the document.
/// </summary>
public sealed partial class ViewerPage
{
    // Highlights are drawn on a canvas of this size that a Viewbox stretches over the page, so zooming never redraws.
    private const double HighlightSpace = 1000;
    private const double AutoScrollStep = 0.25; // share of the distance past the edge scrolled per tick

    private DocumentText? _text;
    private TextPosition _anchor;
    private TextPosition _focus;
    private bool _hasSelection;
    private bool _selecting;
    private Point _dragPoint; // pointer position relative to the scroller while dragging
    private Microsoft.UI.Dispatching.DispatcherQueueTimer? _autoScroll;
    private Brush? _highlightBrush;

    private bool HasSelection => _hasSelection && _anchor != _focus;
    private TextPosition SelectionStart => _anchor < _focus ? _anchor : _focus;
    private TextPosition SelectionEnd => _anchor < _focus ? _focus : _anchor;

    private void InitializeSelection()
    {
        Pages.PointerPressed += OnPagesPressed;
        Pages.PointerMoved += OnPagesMoved;
        Pages.PointerReleased += OnPagesReleased;
        Pages.PointerCaptureLost += (_, _) => EndDrag();
        Pages.PointerExited += (_, _) => { if (!_selecting) ProtectedCursor = null; };
        Pages.DoubleTapped += OnPagesDoubleTapped;
        Pages.RightTapped += OnPagesRightTapped;

        AddTextAccelerator(VirtualKey.C, () => _ = CopySelectionAsync());
        AddTextAccelerator(VirtualKey.A, SelectAll);

        _autoScroll = DispatcherQueue.CreateTimer();
        _autoScroll.Interval = TimeSpan.FromMilliseconds(30);
        _autoScroll.Tick += (_, _) => OnAutoScroll();
    }

    /// <summary>Like the other shortcuts, but leaves Ctrl+C/Ctrl+A to a focused text box.</summary>
    private void AddTextAccelerator(VirtualKey key, Action action)
    {
        var accelerator = new KeyboardAccelerator { Key = key, Modifiers = VirtualKeyModifiers.Control };
        accelerator.Invoked += (_, e) =>
        {
            if (_doc is null || FocusManager.GetFocusedElement(XamlRoot) is TextBox or PasswordBox) return;
            action();
            e.Handled = true;
        };
        KeyboardAccelerators.Add(accelerator);
    }

    private void OpenTextLayer(string? password)
    {
        _text?.Dispose();
        _text = new DocumentText(_path, password, _logger);
        _hasSelection = false;
    }

    private void CloseTextLayer()
    {
        _autoScroll?.Stop();
        _text?.Dispose();
        _text = null;
        _hasSelection = false;
    }

    // ---------------------------------------------------------------- hit testing

    /// <summary>The page under a point relative to the scroller, and the point in that page's fractions.</summary>
    private (int Page, double X, double Y) HitPage(Point scrollerPoint)
    {
        var docY = RealOffset + scrollerPoint.Y;
        var x = Scroller.HorizontalOffset + scrollerPoint.X;
        var page = PageAt(docY);

        // In the gap below a page, the next page is closer when the point is past the middle of the gap.
        if (page + 1 < _tops.Length && docY > _tops[page] + PageHeight(page) + PageGap / 2) page++;

        return (page, (x - PageLeft(page)) / (_sizes[page].Width * _zoom), (docY - _tops[page]) / PageHeight(page));
    }

    /// <summary>The caret under a point; loads the page's text when it is not cached yet.</summary>
    private async Task<TextPosition?> CaretAtAsync(Point scrollerPoint)
    {
        if (_text is not { } text) return null;
        var (page, x, y) = HitPage(scrollerPoint);
        var pageText = text.TryGet(page) ?? await text.GetAsync(page);
        if (text != _text) return null; // the document changed meanwhile
        return new TextPosition(page, pageText.CaretAt(x, y));
    }

    // ---------------------------------------------------------------- pointer

    private static bool IsTextPointer(PointerRoutedEventArgs e) =>
        e.Pointer.PointerDeviceType != PointerDeviceType.Touch;

    private async void OnPagesPressed(object sender, PointerRoutedEventArgs e)
    {
        if (_doc is null || !IsTextPointer(e)) return;
        var point = e.GetCurrentPoint(Scroller);
        if (!point.Properties.IsLeftButtonPressed) return;

        Scroller.Focus(FocusState.Pointer);
        Pages.CapturePointer(e.Pointer);
        e.Handled = true;

        var extend = e.KeyModifiers.HasFlag(VirtualKeyModifiers.Shift) && _hasSelection;
        _selecting = true;
        _dragPoint = point.Position;

        if (await CaretAtAsync(point.Position) is not { } caret) return;
        if (!extend) _anchor = caret;
        _focus = caret;
        _hasSelection = true;
        RedrawSelection();
    }

    private async void OnPagesMoved(object sender, PointerRoutedEventArgs e)
    {
        if (_doc is null || _text is null || !IsTextPointer(e)) return;
        var position = e.GetCurrentPoint(Scroller).Position;

        if (!_selecting)
        {
            UpdateCursor(position);
            return;
        }

        _dragPoint = position;
        var outside = position.Y < 0 || position.Y > Scroller.ViewportHeight;
        if (outside) _autoScroll?.Start();
        else _autoScroll?.Stop();

        await ExtendToAsync(position);
    }

    private void OnPagesReleased(object sender, PointerRoutedEventArgs e)
    {
        if (!_selecting) return;
        Pages.ReleasePointerCapture(e.Pointer);
        EndDrag();
    }

    private void EndDrag()
    {
        _selecting = false;
        _autoScroll?.Stop();
    }

    private void OnAutoScroll()
    {
        if (!_selecting) { _autoScroll?.Stop(); return; }
        var past = _dragPoint.Y < 0 ? _dragPoint.Y : _dragPoint.Y - Scroller.ViewportHeight;
        if (past == 0) return;
        ScrollBy(Math.Clamp(past * AutoScrollStep, -200, 200) + Math.Sign(past) * 4);
        _ = ExtendToAsync(_dragPoint);
    }

    private async Task ExtendToAsync(Point scrollerPoint)
    {
        if (await CaretAtAsync(scrollerPoint) is not { } caret || !_selecting || caret == _focus) return;
        _focus = caret;
        RedrawSelection();
    }

    private void UpdateCursor(Point scrollerPoint)
    {
        var (page, x, y) = HitPage(scrollerPoint);
        if (_text!.TryGet(page) is not { } pageText)
        {
            _ = _text.GetAsync(page); // warm the cache so a press right after has the text
            return;
        }
        var overText = x is >= 0 and <= 1 && pageText.IsOverText(x, y);
        ProtectedCursor = overText ? InputSystemCursor.Create(InputSystemCursorShape.IBeam) : null;
    }

    private async void OnPagesDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (_text is not { } text || e.PointerDeviceType == PointerDeviceType.Touch) return;
        var (page, x, y) = HitPage(e.GetPosition(Scroller));
        var pageText = await text.GetAsync(page);
        var (start, end) = pageText.WordAt(x, y);
        if (start == end || text != _text) return;

        EndDrag();
        _anchor = new TextPosition(page, start);
        _focus = new TextPosition(page, end);
        _hasSelection = true;
        RedrawSelection();
        e.Handled = true;
    }

    private void OnPagesRightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        if (_doc is null) return;
        var copy = new MenuFlyoutItem
        {
            Text = Loc.Get("viewer.copy"),
            Icon = new FontIcon { Glyph = "" },
            IsEnabled = HasSelection,
            KeyboardAcceleratorTextOverride = "Ctrl+C",
        };
        copy.Click += (_, _) => _ = CopySelectionAsync();
        var selectAll = new MenuFlyoutItem
        {
            Text = Loc.Get("viewer.selectAll"),
            Icon = new FontIcon { Glyph = "" },
            KeyboardAcceleratorTextOverride = "Ctrl+A",
        };
        selectAll.Click += (_, _) => SelectAll();

        var menu = new MenuFlyout { Items = { copy, selectAll } };
        menu.ShowAt(Pages, e.GetPosition(Pages));
        e.Handled = true;
    }

    // ---------------------------------------------------------------- selection

    private void SelectAll()
    {
        if (_doc is null) return;
        EndDrag();
        _anchor = new TextPosition(0, 0);
        _focus = new TextPosition(_frames.Length - 1, int.MaxValue); // PageText clamps carets to its length
        _hasSelection = true;
        RedrawSelection();
    }

    /// <summary>The glyph range of a page covered by the selection, or null when the page is outside it.</summary>
    private (int Start, int End)? SelectedRange(int page)
    {
        if (!HasSelection) return null;
        var (start, end) = (SelectionStart, SelectionEnd);
        if (page < start.Page || page > end.Page) return null;
        return (page == start.Page ? start.Caret : 0, page == end.Page ? end.Caret : int.MaxValue);
    }

    private void RedrawSelection()
    {
        for (var i = _shownFrom; i <= _shownTo; i++) DrawHighlights(i);
    }

    /// <summary>Draws the selection on a page's frame; a page whose text is still loading is drawn when it arrives.</summary>
    private void DrawHighlights(int page)
    {
        if (_frames[page] is not { } frame) return;
        var layer = FrameHighlights(frame);
        layer.Children.Clear();
        if (_text is not { } text || SelectedRange(page) is not { } range) return;

        if (text.TryGet(page) is not { } pageText)
        {
            _ = LoadAndDrawAsync(text, page);
            return;
        }

        _highlightBrush ??= new SolidColorBrush((Windows.UI.Color)Application.Current.Resources["SystemAccentColor"]) { Opacity = 0.35 };
        foreach (var box in pageText.Highlight(range.Start, range.End))
        {
            var rect = new Rectangle
            {
                Fill = _highlightBrush,
                Width = (box.Right - box.Left) * HighlightSpace,
                Height = (box.Bottom - box.Top) * HighlightSpace,
            };
            Canvas.SetLeft(rect, box.Left * HighlightSpace);
            Canvas.SetTop(rect, box.Top * HighlightSpace);
            layer.Children.Add(rect);
        }
    }

    private async Task LoadAndDrawAsync(DocumentText text, int page)
    {
        await text.GetAsync(page);
        if (text == _text && page >= _shownFrom && page <= _shownTo) DrawHighlights(page);
    }

    private async Task CopySelectionAsync()
    {
        if (_text is not { } text || !HasSelection) return;
        var (start, end) = (SelectionStart, SelectionEnd);

        var many = end.Page - start.Page > 20;
        if (many) BusyRing.IsActive = true;
        try
        {
            var parts = new List<string>();
            for (var page = start.Page; page <= end.Page && text == _text; page++)
            {
                var range = SelectedRange(page) ?? (0, 0);
                var part = (await text.GetAsync(page)).GetText(range.Start, range.End);
                if (part.Length > 0) parts.Add(part);
            }
            if (text != _text || parts.Count == 0) return;

            var package = new DataPackage();
            package.SetText(string.Join(Environment.NewLine, parts).ReplaceLineEndings());
            Clipboard.SetContent(package);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to copy the selection");
        }
        finally
        {
            if (many) BusyRing.IsActive = false;
        }
    }
}
