using Microsoft.Extensions.Logging;

namespace PdfReader.Services;

/// <summary>
/// Single entry point for "open these PDFs" from the picker, drag and drop, and file lists.
/// Records the files in the library and opens the first one in the viewer.
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
            Notification?.Invoke(this, "Could not show the file picker.");
        }
    }

    public async Task OpenAsync(IEnumerable<string> paths)
    {
        var candidates = paths.ToList();
        var pdfs = files.FilterPdfPaths(candidates);
        if (candidates.Count > 0 && pdfs.Count == 0)
        {
            Notification?.Invoke(this, "Only existing PDF files can be opened.");
            return;
        }

        var added = 0;
        foreach (var path in pdfs)
        {
            try
            {
                await recent.RegisterOpenedAsync(path);
                added++;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to register {Path}", path);
                Notification?.Invoke(this, $"Could not add {Path.GetFileName(path)} to your library.");
            }
        }

        if (added == 0) return;
        LibraryChanged?.Invoke(this, EventArgs.Empty);
        navigation.OpenDocument(pdfs[0]);
    }
}
