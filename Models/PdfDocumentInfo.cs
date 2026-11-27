using CommunityToolkit.Mvvm.ComponentModel;
using PdfReader.Localization;

namespace PdfReader.Models;

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

    public string Details => Loc.Format("doc.details", FormatSize(SizeBytes),
        LastOpened.LocalDateTime.ToString("g", Loc.Service.Culture));

    /// <summary>False until the document has been opened in the viewer and its page count is known.</summary>
    public bool HasProgress => PageCount > 0;

    /// <summary>Share of the document read, counting the page the reader stopped at as read.</summary>
    public int ProgressPercent => HasProgress ? (Math.Clamp(LastPage, 0, PageCount - 1) + 1) * 100 / PageCount : 0;

    public string ProgressText => HasProgress
        ? Loc.Format("doc.progress", Math.Clamp(LastPage, 0, PageCount - 1) + 1, PageCount, ProgressPercent)
        : string.Empty;

    private static string FormatSize(long bytes) => bytes switch
    {
        < 1024 => Loc.Format("size.b", bytes),
        < 1024 * 1024 => Loc.Format("size.kb", (bytes / 1024.0).ToString("0.#", Loc.Service.Culture)),
        _ => Loc.Format("size.mb", (bytes / (1024.0 * 1024.0)).ToString("0.#", Loc.Service.Culture)),
    };
}
