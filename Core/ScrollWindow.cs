namespace PdfReader.Core;

/// <summary>
/// XAML cannot scroll past ~1M DIPs, so a long document is shown through a window of <see cref="Extent"/> DIPs
/// that slides along it. Offsets inside the window stay 1:1 with the document, whatever its length.
/// </summary>
public static class ScrollWindow
{
    public const double Extent = 600_000;
    public const double Margin = 150_000;

    /// <summary>Returns where the window should start to show <paramref name="offset"/>; <paramref name="start"/> if it can stay.</summary>
    public static double Place(double start, double offset, double total)
    {
        var maxStart = Math.Max(0, total - Extent);
        var local = offset - start;
        var fits = start <= maxStart
            && local >= (start > 0 ? Margin : 0)
            && local <= (start < maxStart ? Extent - Margin : Extent);
        return fits ? start : Math.Clamp(offset - Extent / 2, 0, maxStart);
    }
}
