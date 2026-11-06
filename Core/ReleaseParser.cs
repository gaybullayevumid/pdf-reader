using System.Text.Json;
using PdfReader.Models;

namespace PdfReader.Core;

/// <summary>Reads a GitHub "latest release" document and picks the installer asset.</summary>
public static class ReleaseParser
{
    public const string InstallerPrefix = "PDFReader-Setup-";

    /// <param name="isTrustedUrl">Decides whether an asset URL may be downloaded and executed.</param>
    public static UpdateInfo? Parse(JsonElement release, Func<string, bool> isTrustedUrl)
    {
        if (release.TryGetProperty("draft", out var draft) && draft.ValueKind == JsonValueKind.True) return null;
        if (release.TryGetProperty("prerelease", out var pre) && pre.ValueKind == JsonValueKind.True) return null;
        if (!release.TryGetProperty("tag_name", out var tagElement)) return null;

        if (!TryParseVersion(tagElement.GetString(), out var version)) return null;
        if (!release.TryGetProperty("assets", out var assets) || assets.ValueKind != JsonValueKind.Array) return null;

        foreach (var asset in assets.EnumerateArray())
        {
            var name = asset.TryGetProperty("name", out var n) ? n.GetString() ?? string.Empty : string.Empty;
            if (!name.StartsWith(InstallerPrefix, StringComparison.OrdinalIgnoreCase) ||
                !name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) continue;

            var url = asset.TryGetProperty("browser_download_url", out var u) ? u.GetString() ?? string.Empty : string.Empty;
            if (!isTrustedUrl(url)) continue;

            string? sha = null;
            if (asset.TryGetProperty("digest", out var digest) &&
                digest.GetString() is { } d && d.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase))
                sha = d["sha256:".Length..];

            var page = release.TryGetProperty("html_url", out var html) ? html.GetString() ?? url : url;
            return new UpdateInfo(version, url, name, page, sha);
        }
        return null;
    }

    /// <summary>"v1.2.0" → 1.2.0. Anything that is not a version returns false.</summary>
    public static bool TryParseVersion(string? tag, out Version version)
    {
        version = new Version(0, 0, 0);
        if (string.IsNullOrWhiteSpace(tag) || !Version.TryParse(tag.Trim().TrimStart('v', 'V'), out var parsed)) return false;
        version = Normalize(parsed);
        return true;
    }

    public static Version Normalize(Version? v) =>
        v is null ? new Version(0, 0, 0) : new Version(v.Major, Math.Max(0, v.Minor), Math.Max(0, v.Build));
}
