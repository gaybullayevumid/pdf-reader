using Microsoft.Extensions.Logging;

namespace PdfReader.Services;

/// <summary>
/// Single entry point for "open these PDFs" from the picker, drag and drop, and file lists.
/// Records the files in the library and opens each one in a tab.
/// </summary>
public interface IDocumentOpener
{
    event EventHandler? LibraryChanged;
    event EventHandler<string>? Notification;

    Task OpenAsync(IEnumerable<string> paths);
    Task PickAndOpenAsync();
}

public sealed class DocumentOpener(
    IFileService files,
    IRecentFilesService recent,
    INavigationService navigation,
    IDocumentTabService tabs,
    ILocalizationService loc,
    ILogger<DocumentOpener> logger) : IDocumentOpener
{
    public event EventHandler? LibraryChanged;
    public event EventHandler<string>? Notification;

    public async Task PickAndOpenAsync()
    {
        try
        {
            await OpenAsync(await files.PickPdfFilesAsync());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "File picker failed");
            Notification?.Invoke(this, loc.Get("opener.pickerFailed"));
        }
    }

    public async Task OpenAsync(IEnumerable<string> paths)
    {
        var candidates = paths.ToList();
        var pdfs = files.FilterPdfPaths(candidates);
        if (candidates.Count > 0 && pdfs.Count == 0)
        {
            Notification?.Invoke(this, loc.Get("opener.onlyPdf"));
            return;
        }

        var opened = new List<string>();
        foreach (var path in pdfs)
        {
            try
            {
                await recent.RegisterOpenedAsync(path);
                opened.Add(path);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to register {Path}", path);
                Notification?.Invoke(this, loc.Format("opener.addFailed", Path.GetFileName(path)));
            }
        }

        if (opened.Count == 0) return;
        LibraryChanged?.Invoke(this, EventArgs.Empty);

        // Every file gets a tab (or keeps its existing one). The first is shown; the rest open behind it, in
        // order, and load only when their tab is first shown.
        navigation.OpenDocument(opened[0]);
        for (var i = opened.Count - 1; i >= 1; i--) tabs.Open(opened[i], activate: false);
    }
}
