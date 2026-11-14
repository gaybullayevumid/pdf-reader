using System.Text.Json;

namespace PdfReader.Core;

/// <summary>
/// Where a document was being read: the page at the top of the viewport and how far into it, plus the zoom.
/// Positions are stored relative to a page, so they survive window size and zoom changes.
/// </summary>
public sealed record ViewState(
    int Page = 0,
    double PageFraction = 0,
    double Zoom = 1,
    bool FitWidth = true,
    double HorizontalOffset = 0);

/// <summary>One open tab as persisted between runs.</summary>
public sealed record TabEntry(string Path, ViewState? View = null);

/// <summary>The open tabs, in order, and the one that was showing (-1 when a library page was showing).</summary>
public sealed record SessionData(List<TabEntry> Tabs, int Active = -1)
{
    public static SessionData Empty => new([]);
}

/// <summary>Tab list rules shared by the tab service and its tests.</summary>
public static class TabRules
{
    /// <summary>Canonical form of a path, so the same file opened twice is recognised as one tab.</summary>
    public static string Normalize(string path)
    {
        try { return System.IO.Path.GetFullPath(path.Trim().Trim('"')); }
        catch (Exception) { return path; }
    }

    public static bool SamePath(string a, string b) =>
        string.Equals(Normalize(a), Normalize(b), StringComparison.OrdinalIgnoreCase);

    public static int IndexOf(IReadOnlyList<string> paths, string path)
    {
        for (var i = 0; i < paths.Count; i++)
            if (SamePath(paths[i], path)) return i;
        return -1;
    }

    /// <summary>
    /// The tab to show after closing tab <paramref name="closed"/> of <paramref name="count"/> (count before closing),
    /// as an index into the list after the close; -1 when no tab should be active.
    /// Like browsers: closing the active tab activates its right neighbour, or the left one at the end.
    /// </summary>
    public static int ActiveAfterClose(int closed, int active, int count)
    {
        if (count <= 1 || active < 0) return -1;
        if (closed < active) return active - 1;
        if (closed > active) return active;
        return Math.Min(closed, count - 2);
    }
}

/// <summary>Reads and writes <see cref="SessionData"/>; a missing or damaged file is an empty session.</summary>
public static class SessionStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static SessionData Load(string path, Action<Exception>? onError = null)
    {
        try
        {
            if (File.Exists(path) && JsonSerializer.Deserialize<SessionData>(File.ReadAllText(path), Options) is { } data)
            {
                var tabs = (data.Tabs ?? []).Where(t => !string.IsNullOrWhiteSpace(t?.Path)).ToList();
                return new SessionData(tabs, data.Active >= 0 && data.Active < tabs.Count ? data.Active : -1);
            }
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex);
        }
        return SessionData.Empty;
    }

    /// <summary>Writes to a temp file first so a crash mid-write cannot leave a half-written session.</summary>
    public static void Save(string path, SessionData session)
    {
        var temp = path + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(session, Options));
        File.Move(temp, path, overwrite: true);
    }
}
