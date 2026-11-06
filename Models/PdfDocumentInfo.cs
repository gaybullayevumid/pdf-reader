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

    private static string FormatSize(long bytes) => bytes switch
    {
        < 1024 => Loc.Format("size.b", bytes),
        < 1024 * 1024 => Loc.Format("size.kb", (bytes / 1024.0).ToString("0.#", Loc.Service.Culture)),
        _ => Loc.Format("size.mb", (bytes / (1024.0 * 1024.0)).ToString("0.#", Loc.Service.Culture)),
    };
}
