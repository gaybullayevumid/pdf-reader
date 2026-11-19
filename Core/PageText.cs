namespace PdfReader.Core;

/// <summary>An axis-aligned box in page fractions: 0..1 across and down the page as it is displayed.</summary>
public readonly record struct TextRect(double Left, double Top, double Right, double Bottom)
{
    public double Height => Bottom - Top;
    public bool Contains(double x, double y) => x >= Left && x <= Right && y >= Top && y <= Bottom;
    public TextRect Union(TextRect o) => new(Math.Min(Left, o.Left), Math.Min(Top, o.Top), Math.Max(Right, o.Right), Math.Max(Bottom, o.Bottom));
}

/// <summary>One character (or ligature) as drawn on the page.</summary>
public readonly record struct Glyph(string Text, TextRect Box);

/// <summary>
/// A clickable area of a page: either a web/mail address (<see cref="Uri"/>) or a page of this document
/// (<see cref="TargetPage"/>, zero-based, with <see cref="TargetTop"/> the destination's top in PDF points when known).
/// </summary>
public sealed record PageLink(TextRect Box, string? Uri, int? TargetPage, double? TargetTop);

/// <summary>A point in the document's text: before glyph <see cref="Caret"/> of page <see cref="Page"/>.</summary>
public readonly record struct TextPosition(int Page, int Caret) : IComparable<TextPosition>
{
    public int CompareTo(TextPosition other) => Page != other.Page ? Page.CompareTo(other.Page) : Caret.CompareTo(other.Caret);
    public static bool operator <(TextPosition a, TextPosition b) => a.CompareTo(b) < 0;
    public static bool operator >(TextPosition a, TextPosition b) => a.CompareTo(b) > 0;
    public static bool operator <=(TextPosition a, TextPosition b) => a.CompareTo(b) <= 0;
    public static bool operator >=(TextPosition a, TextPosition b) => a.CompareTo(b) >= 0;
}

/// <summary>
/// The text layer of one page: glyphs grouped into lines in drawing order, with the hit testing, highlighting and
/// text building that selection needs. Carets are glyph boundaries, 0..<see cref="Count"/>.
/// </summary>
public sealed class PageText
{
    // A glyph joins the current line when it overlaps it vertically by this share of the smaller height.
    private const double LineOverlap = 0.5;
    // A horizontal gap wider than this share of the line height reads as a word break.
    private const double SpaceGap = 0.15;

    public static readonly PageText Empty = new([], [], [], []);

    private readonly Glyph[] _glyphs;
    private readonly string[] _separators; // text inserted before each glyph: "", " " or "\n"
    private readonly Line[] _lines;
    private readonly PageLink[] _links;

    private readonly record struct Line(int Start, int End, TextRect Box); // glyphs [Start, End)

    private PageText(Glyph[] glyphs, string[] separators, Line[] lines, PageLink[] links)
    {
        _glyphs = glyphs;
        _separators = separators;
        _lines = lines;
        _links = links;
    }

    public int Count => _glyphs.Length;

    /// <summary>Builds the text layer from glyphs in the order the page draws them; empty glyphs are dropped.</summary>
    public static PageText Build(IEnumerable<Glyph> drawn, IEnumerable<PageLink>? links = null)
    {
        var glyphs = drawn.Where(g => !string.IsNullOrEmpty(g.Text)).ToArray();
        PageLink[] linkArray = links is null ? [] : [.. links];
        if (glyphs.Length == 0) return linkArray.Length == 0 ? Empty : new PageText([], [], [], linkArray);

        var separators = new string[glyphs.Length];
        var lines = new List<Line>();
        var start = 0;
        var box = glyphs[0].Box;
        separators[0] = "";

        for (var i = 1; i < glyphs.Length; i++)
        {
            var g = glyphs[i].Box;
            var previous = glyphs[i - 1];
            var overlap = Math.Min(box.Bottom, g.Bottom) - Math.Max(box.Top, g.Top);
            var sameLine = overlap >= LineOverlap * Math.Min(box.Height, g.Height)
                && g.Left >= previous.Box.Right - box.Height; // text running backwards starts a new line

            if (sameLine)
            {
                var gap = g.Left - previous.Box.Right;
                separators[i] = gap > SpaceGap * box.Height && !IsSpace(previous.Text) && !IsSpace(glyphs[i].Text) ? " " : "";
                box = box.Union(g);
            }
            else
            {
                lines.Add(new Line(start, i, box));
                separators[i] = "\n";
                start = i;
                box = g;
            }
        }
        lines.Add(new Line(start, glyphs.Length, box));
        return new PageText(glyphs, separators, [.. lines], linkArray);
    }

    /// <summary>The link under a point, or null. Later annotations are drawn on top, so they win.</summary>
    public PageLink? LinkAt(double x, double y)
    {
        for (var i = _links.Length - 1; i >= 0; i--)
            if (_links[i].Box.Contains(x, y)) return _links[i];
        return null;
    }

    /// <summary>True when the point is on a line of text (used for the I-beam cursor).</summary>
    public bool IsOverText(double x, double y) => _lines.Any(l => l.Box.Contains(x, y));

    /// <summary>The caret nearest to a point; above all text it is 0, below all text it is <see cref="Count"/>.</summary>
    public int CaretAt(double x, double y)
    {
        if (_lines.Length == 0) return 0;
        if (_lines.All(l => y < l.Box.Top)) return 0;
        if (_lines.All(l => y > l.Box.Bottom)) return Count;

        var line = NearestLine(x, y);
        for (var i = line.Start; i < line.End; i++)
        {
            var b = _glyphs[i].Box;
            if (x < (b.Left + b.Right) / 2) return i;
        }
        return line.End;
    }

    /// <summary>The glyph range [start, end) of the word under a point, or an empty range.</summary>
    public (int Start, int End) WordAt(double x, double y)
    {
        var hit = Array.FindIndex(_glyphs, g => g.Box.Contains(x, y));
        if (hit < 0) return (0, 0);
        if (!IsWordChar(hit)) return (hit, hit + 1);

        var start = hit;
        while (start > 0 && _separators[start] == "" && IsWordChar(start - 1)) start--;
        var end = hit + 1;
        while (end < Count && _separators[end] == "" && IsWordChar(end)) end++;
        return (start, end);
    }

    /// <summary>One box per line covering the selected glyphs of [start, end).</summary>
    public IEnumerable<TextRect> Highlight(int start, int end)
    {
        (start, end) = Clamp(start, end);
        foreach (var line in _lines)
        {
            var from = Math.Max(start, line.Start);
            var to = Math.Min(end, line.End);
            if (from >= to) continue;
            yield return new TextRect(_glyphs[from].Box.Left, line.Box.Top, _glyphs[to - 1].Box.Right, line.Box.Bottom);
        }
    }

    /// <summary>The text of [start, end) with word breaks as spaces and line breaks as newlines.</summary>
    public string GetText(int start, int end)
    {
        (start, end) = Clamp(start, end);
        var text = new System.Text.StringBuilder();
        for (var i = start; i < end; i++)
        {
            if (i > start) text.Append(_separators[i]);
            text.Append(_glyphs[i].Text);
        }
        return text.ToString();
    }

    private Line NearestLine(double x, double y)
    {
        // Prefer lines at this height (several when there are columns), then the vertically closest one.
        var best = _lines[0];
        var bestScore = double.MaxValue;
        foreach (var line in _lines)
        {
            var dy = y < line.Box.Top ? line.Box.Top - y : y > line.Box.Bottom ? y - line.Box.Bottom : 0;
            var dx = x < line.Box.Left ? line.Box.Left - x : x > line.Box.Right ? x - line.Box.Right : 0;
            var score = dy * 1000 + dx;
            if (score < bestScore) { bestScore = score; best = line; }
        }
        return best;
    }

    private (int, int) Clamp(int start, int end)
    {
        start = Math.Clamp(start, 0, Count);
        end = Math.Clamp(end, 0, Count);
        return start <= end ? (start, end) : (end, start);
    }

    private bool IsWordChar(int i) => _glyphs[i].Text.Any(char.IsLetterOrDigit);

    private static bool IsSpace(string text) => text.All(char.IsWhiteSpace);
}
