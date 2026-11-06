namespace PdfReader.Models;

/// <summary>A newer release published on GitHub.</summary>
public sealed record UpdateInfo(
    Version Version,
    string InstallerUrl,
    string InstallerName,
    string PageUrl,
    string? Sha256);

public enum UpdateCheckResult
{
    UpToDate,
    Available,
    Failed,
}
