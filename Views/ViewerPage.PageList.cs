using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using PdfReader.Services;
using Windows.Data.Pdf;
using Windows.Storage.Streams;

namespace PdfReader.Views;

/// <summary>One entry of the page list: a placeholder sized like the page, filled with a thumbnail while it is on screen.</summary>
public sealed partial class PageThumbnail(int index, double width, double height) : ObservableObject
{
    public int Index { get; } = index;
    public string Label { get; } = (index + 1).ToString(CultureInfo.CurrentCulture);
    public double Width { get; } = width;
    public double Height { get; } = height;

    [ObservableProperty]
    public partial ImageSource? Image { get; set; }

    /// <summary>Bumped when the item leaves the screen or is rendered again, so a late render is dropped.</summary>
    internal int Generation { get; set; }
}

/// <summary>
/// Page list on the left: a thumbnail per page that follows the current page and jumps to a page when picked.
/// Only items the list has realized hold a bitmap; recycled ones drop it, so long documents stay cheap.
/// </summary>
public sealed partial class ViewerPage
{
    private const double ThumbnailWidth = 112;
    private const double ThumbnailMaxHeight = 160;

    private readonly ISettingsService _settings = App.Services.GetRequiredService<ISettingsService>();
    private readonly SemaphoreSlim _thumbnailGate = new(1);
    private bool _updatingList;

    private bool IsPageListShown => PageListPane.Visibility == Visibility.Visible;

    /// <summary>Shows or hides the list as the setting says; another tab may have changed it.</summary>
    private void ApplyPageListSetting()
    {
        var show = _settings.ShowPageList;
        PageListToggle.IsChecked = show;
        PageListPane.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
    }

    private void FillPageList()
    {
        PageList.ItemsSource = _sizes
            .Select((size, i) =>
            {
                var scale = Math.Min(ThumbnailWidth / size.Width, ThumbnailMaxHeight / size.Height);
                return new PageThumbnail(i, size.Width * scale, size.Height * scale);
            })
            .ToList();
    }

    private void ClearPageList() => PageList.ItemsSource = null;

    /// <summary>Selects the current page in the list and keeps it in view.</summary>
    private void SyncPageList()
    {
        if (PageList.ItemsSource is null) return;
        _updatingList = true;
        PageList.SelectedIndex = _currentPage;
        _updatingList = false;
        if (IsPageListShown && PageList.SelectedItem is { } item) PageList.ScrollIntoView(item);
    }

    private void OnPageListToggle(object sender, RoutedEventArgs e)
    {
        var show = PageListToggle.IsChecked == true;
        _settings.ShowPageList = show;
        PageListPane.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        if (show) SyncPageList();
    }

    private void OnPageListSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_updatingList || PageList.SelectedIndex < 0) return;
        ScrollToPage(PageList.SelectedIndex);
    }

    private void OnPageListContainerChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
        if (args.Item is not PageThumbnail item) return;
        if (args.InRecycleQueue)
        {
            item.Generation++;
            item.Image = null;
        }
        else if (item.Image is null)
        {
            _ = RenderThumbnailAsync(item);
        }
    }

    private async Task RenderThumbnailAsync(PageThumbnail item)
    {
        var doc = _doc;
        if (doc is null) return;

        var generation = ++item.Generation;
        var entered = false;
        try
        {
            await _thumbnailGate.WaitAsync();
            entered = true;
            if (generation != item.Generation || doc != _doc) return; // scrolled away while queued

            var scale = XamlRoot?.RasterizationScale ?? 1;
            using var stream = new InMemoryRandomAccessStream();
            using (var page = doc.GetPage((uint)item.Index))
            {
                await page.RenderToStreamAsync(stream, new PdfPageRenderOptions
                {
                    DestinationWidth = (uint)Math.Max(1, Math.Ceiling(item.Width * scale)),
                    DestinationHeight = (uint)Math.Max(1, Math.Ceiling(item.Height * scale)),
                });
            }
            if (generation != item.Generation || doc != _doc) return;

            var bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(stream);
            if (generation != item.Generation || doc != _doc) return;

            item.Image = bitmap;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to render the thumbnail of page {Page}", item.Index + 1);
        }
        finally
        {
            if (entered) _thumbnailGate.Release();
        }
    }
}
