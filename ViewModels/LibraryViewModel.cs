using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PdfReader.Models;
using PdfReader.Services;

namespace PdfReader.ViewModels;

/// <summary>Backs both the Home page (recent files) and the list pages (Recent, All PDFs, Favorites, Bookmarks).</summary>
public sealed partial class LibraryViewModel(
    IRecentFilesService recent,
    IDocumentOpener opener,
    ILocalizationService loc,
    ILogger<LibraryViewModel> logger) : ObservableObject
{
    private LibraryFilter _filter = LibraryFilter.Recent;
    private int _limit = 200;
    private CancellationTokenSource? _loadCts;

    public ObservableCollection<PdfDocumentInfo> Documents { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    public partial bool IsLoaded { get; set; }

    public bool IsEmpty => IsLoaded && Documents.Count == 0;

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EmptyTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EmptyDescription { get; set; } = string.Empty;

    public void Activate(LibraryFilter filter, int limit = 200)
    {
        _filter = filter;
        _limit = limit;
        UpdateTexts();
        opener.LibraryChanged += OnLibraryChanged;
        loc.LanguageChanged += OnLanguageChanged;
        _ = RefreshAsync();
    }

    private void UpdateTexts()
    {
        var key = _filter switch
        {
            LibraryFilter.Recent => "recent",
            LibraryFilter.All => "all",
            LibraryFilter.Favorites => "favorites",
            _ => "bookmarks",
        };
        Title = loc.Get($"lib.{key}.title");
        EmptyTitle = loc.Get($"lib.{key}.emptyTitle");
        EmptyDescription = loc.Get($"lib.{key}.emptyDesc");
    }

    // Dates and sizes in the list are formatted per language, so rebuild the items too.
    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        UpdateTexts();
        _ = RefreshAsync();
    }

    public void Deactivate()
    {
        opener.LibraryChanged -= OnLibraryChanged;
        loc.LanguageChanged -= OnLanguageChanged;
        _loadCts?.Cancel();
    }

    private void OnLibraryChanged(object? sender, EventArgs e) => _ = RefreshAsync();

    [RelayCommand]
    private async Task RefreshAsync()
    {
        _loadCts?.Cancel();
        var cts = _loadCts = new CancellationTokenSource();
        try
        {
            var items = await recent.QueryAsync(_filter, _limit, cts.Token);
            if (cts.IsCancellationRequested) return;
            Documents.Clear();
            foreach (var item in items) Documents.Add(item);
            IsLoaded = true;
            OnPropertyChanged(nameof(IsEmpty));
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load library ({Filter})", _filter);
        }
    }

    public Task OpenAsync(PdfDocumentInfo document) => opener.OpenAsync([document.Path]);

    public Task OpenManyAsync(IEnumerable<string> paths) => opener.OpenAsync(paths);

    [RelayCommand]
    private Task PickAsync() => opener.PickAndOpenAsync();

    public async Task ToggleFavoriteAsync(PdfDocumentInfo document)
    {
        var value = !document.IsFavorite;
        try
        {
            await recent.SetFavoriteAsync(document.Path, value);
            document.IsFavorite = value;
            if (_filter == LibraryFilter.Favorites && !value) Documents.Remove(document);
            OnPropertyChanged(nameof(IsEmpty));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to toggle favorite for {Path}", document.Path);
        }
    }

    public async Task RemoveAsync(PdfDocumentInfo document)
    {
        try
        {
            await recent.RemoveAsync(document.Path);
            Documents.Remove(document);
            OnPropertyChanged(nameof(IsEmpty));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to remove {Path}", document.Path);
        }
    }
}
