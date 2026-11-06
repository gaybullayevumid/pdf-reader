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
        (Title, EmptyTitle, EmptyDescription) = filter switch
        {
            LibraryFilter.Recent => ("Recent files", "No recent files", "PDFs you open will show up here."),
            LibraryFilter.All => ("All PDFs", "No PDFs yet", "Open a PDF or drop one into the window to build your library."),
            LibraryFilter.Favorites => ("Favorites", "No favorites yet", "Select the star on any PDF to keep it here."),
            _ => ("Bookmarks", "No bookmarks yet", "Documents with bookmarks will be listed here."),
        };
        opener.LibraryChanged += OnLibraryChanged;
        _ = RefreshAsync();
    }

    public void Deactivate()
    {
        opener.LibraryChanged -= OnLibraryChanged;
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
