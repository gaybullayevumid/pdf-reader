using PaperFlow.Database;
using PaperFlow.Models;

namespace PaperFlow.Services;

public sealed class RecentFilesService(AppDatabase database) : IRecentFilesService
{
    public Task<IReadOnlyList<PdfDocumentInfo>> QueryAsync(LibraryFilter filter, int limit = 200, CancellationToken ct = default) =>
        Task.Run<IReadOnlyList<PdfDocumentInfo>>(() =>
        {
            using var connection = database.Open();
            using var command = connection.CreateCommand();
            var where = filter switch
            {
                LibraryFilter.Favorites => "WHERE is_favorite = 1",
                LibraryFilter.Bookmarks => "WHERE path IN (SELECT document_path FROM bookmarks)",
                _ => string.Empty,
            };
            var order = filter == LibraryFilter.All ? "name COLLATE NOCASE ASC" : "last_opened DESC";
            command.CommandText =
                $"SELECT path, name, size_bytes, last_opened, last_page, page_count, is_favorite FROM documents {where} ORDER BY {order} LIMIT $limit";
            command.Parameters.AddWithValue("$limit", limit);

            var result = new List<PdfDocumentInfo>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                ct.ThrowIfCancellationRequested();
                result.Add(ReadDocument(reader));
            }
            return result;
        }, ct);

    public Task<PdfDocumentInfo?> GetAsync(string path, CancellationToken ct = default) =>
        Task.Run(() =>
        {
            using var connection = database.Open();
            using var command = connection.CreateCommand();
            command.CommandText =
                "SELECT path, name, size_bytes, last_opened, last_page, page_count, is_favorite FROM documents WHERE path = $path";
            command.Parameters.AddWithValue("$path", path);
            using var reader = command.ExecuteReader();
            return reader.Read() ? ReadDocument(reader) : null;
        }, ct);

    public Task UpdateProgressAsync(string path, int page, int pageCount, CancellationToken ct = default) =>
        Task.Run(() =>
        {
            using var connection = database.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE documents SET last_page = $page, page_count = $count WHERE path = $path";
            command.Parameters.AddWithValue("$page", page);
            command.Parameters.AddWithValue("$count", pageCount);
            command.Parameters.AddWithValue("$path", path);
            command.ExecuteNonQuery();
        }, ct);

    private static PdfDocumentInfo ReadDocument(Microsoft.Data.Sqlite.SqliteDataReader reader) => new()
    {
        Path = reader.GetString(0),
        Name = reader.GetString(1),
        SizeBytes = reader.GetInt64(2),
        LastOpened = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(3)),
        LastPage = reader.GetInt32(4),
        PageCount = reader.GetInt32(5),
        IsFavorite = reader.GetInt32(6) != 0,
    };

    public Task RegisterOpenedAsync(string path, CancellationToken ct = default) =>
        Task.Run(() =>
        {
            var info = new FileInfo(path);
            using var connection = database.Open();
            using var command = connection.CreateCommand();
            command.CommandText = """
                INSERT INTO documents (path, name, size_bytes, last_opened)
                VALUES ($path, $name, $size, $now)
                ON CONFLICT(path) DO UPDATE SET
                    name = excluded.name, size_bytes = excluded.size_bytes, last_opened = excluded.last_opened
                """;
            command.Parameters.AddWithValue("$path", path);
            command.Parameters.AddWithValue("$name", info.Name);
            command.Parameters.AddWithValue("$size", info.Exists ? info.Length : 0);
            command.Parameters.AddWithValue("$now", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            command.ExecuteNonQuery();
        }, ct);

    public Task SetFavoriteAsync(string path, bool isFavorite, CancellationToken ct = default) =>
        Execute("UPDATE documents SET is_favorite = $v WHERE path = $path", path, isFavorite ? 1 : 0, ct);

    public Task RemoveAsync(string path, CancellationToken ct = default) =>
        Execute("DELETE FROM documents WHERE path = $path", path, null, ct);

    private Task Execute(string sql, string path, object? value, CancellationToken ct) =>
        Task.Run(() =>
        {
            using var connection = database.Open();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("$path", path);
            if (value is not null) command.Parameters.AddWithValue("$v", value);
            command.ExecuteNonQuery();
        }, ct);
}
