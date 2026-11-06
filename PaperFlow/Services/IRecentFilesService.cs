using PaperFlow.Models;

namespace PaperFlow.Services;

public interface IRecentFilesService
{
    Task<IReadOnlyList<PdfDocumentInfo>> QueryAsync(LibraryFilter filter, int limit = 200, CancellationToken ct = default);
    Task<PdfDocumentInfo?> GetAsync(string path, CancellationToken ct = default);
    Task UpdateProgressAsync(string path, int page, int pageCount, CancellationToken ct = default);
    Task RegisterOpenedAsync(string path, CancellationToken ct = default);
    Task SetFavoriteAsync(string path, bool isFavorite, CancellationToken ct = default);
    Task RemoveAsync(string path, CancellationToken ct = default);
}
