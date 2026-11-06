using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace PdfReader.Database;

/// <summary>Owns the SQLite file and schema. Connections are short-lived (pooled by Microsoft.Data.Sqlite).</summary>
public sealed class AppDatabase
{
    private readonly string _connectionString;
    private readonly ILogger<AppDatabase> _logger;

    public AppDatabase(ILogger<AppDatabase> logger)
    {
        _logger = logger;
        var path = Path.Combine(AppPaths.DataDirectory, "pdfreader.db");
        _connectionString = new SqliteConnectionStringBuilder { DataSource = path }.ToString();
    }

    public SqliteConnection Open()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    public void Initialize()
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS documents (
                path        TEXT PRIMARY KEY NOT NULL,
                name        TEXT NOT NULL,
                size_bytes  INTEGER NOT NULL DEFAULT 0,
                last_opened INTEGER NOT NULL,
                last_page   INTEGER NOT NULL DEFAULT 0,
                page_count  INTEGER NOT NULL DEFAULT 0,
                zoom        REAL NOT NULL DEFAULT 1.0,
                is_favorite INTEGER NOT NULL DEFAULT 0
            );
            CREATE INDEX IF NOT EXISTS ix_documents_last_opened ON documents(last_opened DESC);

            CREATE TABLE IF NOT EXISTS bookmarks (
                id            INTEGER PRIMARY KEY AUTOINCREMENT,
                document_path TEXT NOT NULL REFERENCES documents(path) ON DELETE CASCADE,
                page          INTEGER NOT NULL,
                title         TEXT NOT NULL,
                created       INTEGER NOT NULL
            );
            """;
        command.ExecuteNonQuery();
        _logger.LogInformation("Database ready");
    }
}
