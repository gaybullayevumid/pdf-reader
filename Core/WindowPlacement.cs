namespace PdfReader.Core;

public enum WindowMode
{
    Normal,
    Maximized,
    FullScreen,
}

public readonly record struct RectI(int X, int Y, int Width, int Height)
{
    public int Right => X + Width;
    public int Bottom => Y + Height;

    public RectI? Intersect(RectI other)
    {
        var left = Math.Max(X, other.X);
        var top = Math.Max(Y, other.Y);
        var right = Math.Min(Right, other.Right);
        var bottom = Math.Min(Bottom, other.Bottom);
        return right > left && bottom > top ? new RectI(left, top, right - left, bottom - top) : null;
    }
}

/// <summary>
/// What is persisted about the main window. Bounds always describe the *restored* (normal) window, so
/// un-maximizing or leaving full screen returns to the size the user chose.
/// </summary>
public sealed record WindowStateData(
    WindowMode Mode = WindowMode.Normal,
    int? X = null,
    int? Y = null,
    int Width = WindowPlacement.DefaultWidth,
    int Height = WindowPlacement.DefaultHeight,
    bool WasMaximized = false);

/// <summary>Turns saved (possibly stale or corrupt) window data into something safe to apply.</summary>
public static class WindowPlacement
{
    public const int DefaultWidth = 1280;
    public const int DefaultHeight = 820;
    public const int MinWidth = 640;
    public const int MinHeight = 420;

    private const int MaxReasonableSize = 30000;
    private const int MinVisibleWidth = 160;
    private const int MinVisibleHeight = 80;

    /// <param name="workAreas">Usable rectangles of the connected displays, primary first. May be empty.</param>
    public static WindowStateData Sanitize(WindowStateData? data, IReadOnlyList<RectI> workAreas)
    {
        data ??= new WindowStateData();

        var mode = Enum.IsDefined(data.Mode) ? data.Mode : WindowMode.Normal;
        var width = data.Width is > 0 and <= MaxReasonableSize ? data.Width : DefaultWidth;
        var height = data.Height is > 0 and <= MaxReasonableSize ? data.Height : DefaultHeight;
        width = Math.Max(width, MinWidth);
        height = Math.Max(height, MinHeight);

        if (workAreas.Count == 0)
            return data with { Mode = mode, Width = width, Height = height, X = null, Y = null };

        var area = BestArea(data, width, height, workAreas);

        width = Math.Min(width, area.Width);
        height = Math.Min(height, area.Height);

        int x, y;
        if (data.X is { } savedX && data.Y is { } savedY && IsVisibleEnough(new RectI(savedX, savedY, width, height), workAreas))
        {
            // Keep it where the user left it, but fully on the display it mostly sits on.
            x = Math.Clamp(savedX, area.X, Math.Max(area.X, area.Right - width));
            y = Math.Clamp(savedY, area.Y, Math.Max(area.Y, area.Bottom - height));
        }
        else
        {
            x = area.X + (area.Width - width) / 2;
            y = area.Y + (area.Height - height) / 2;
        }

        return new WindowStateData(mode, x, y, width, height, data.WasMaximized);
    }

    private static RectI BestArea(WindowStateData data, int width, int height, IReadOnlyList<RectI> workAreas)
    {
        if (data.X is not { } x || data.Y is not { } y) return workAreas[0];

        var saved = new RectI(x, y, width, height);
        var best = workAreas[0];
        long bestOverlap = 0;
        foreach (var area in workAreas)
        {
            if (saved.Intersect(area) is not { } overlap) continue;
            long size = (long)overlap.Width * overlap.Height;
            if (size > bestOverlap)
            {
                bestOverlap = size;
                best = area;
            }
        }
        return best;
    }

    private static bool IsVisibleEnough(RectI window, IReadOnlyList<RectI> workAreas) =>
        workAreas.Any(a => window.Intersect(a) is { } v &&
                           v.Width >= Math.Min(MinVisibleWidth, window.Width) &&
                           v.Height >= Math.Min(MinVisibleHeight, window.Height));
}
