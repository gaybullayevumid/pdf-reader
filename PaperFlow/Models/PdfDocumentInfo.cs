using CommunityToolkit.Mvvm.ComponentModel;

namespace PaperFlow.Models;

public sealed partial class PdfDocumentInfo : ObservableObject
{
    public required string Path { get; init; }
    public required string Name { get; init; }
    public long SizeBytes { get; init; }
    public DateTimeOffset LastOpened { get; init; }
    public int LastPage { get; init; }
    public int PageCount { get; init; }

    [ObservableProperty]
    public partial bool IsFavorite { get; set; }

    public bool Exists => File.Exists(Path);

    public string Folder => System.IO.Path.GetDirectoryName(Path) ?? string.Empty;

    public string Details =>
        $"{FormatSize(SizeBytes)} · Opened {LastOpened.LocalDateTime:g}";

    private static string FormatSize(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:0.#} KB",
        _ => $"{bytes / (1024.0 * 1024.0):0.#} MB",
    };
}
