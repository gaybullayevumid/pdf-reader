using PdfReader.Core;

namespace PdfReader.Tests;

public class PageTextTests
{
    private const double H = 0.02; // line height
    private const double W = 0.01; // glyph width

    /// <summary>Lays out words left to right on a line whose top is <paramref name="top"/>; words are a glyph apart.</summary>
    private static IEnumerable<Glyph> Line(double top, double left, params string[] words)
    {
        var x = left;
        foreach (var word in words)
        {
            foreach (var c in word)
            {
                yield return new Glyph(c.ToString(), new TextRect(x, top, x + W, top + H));
                x += W;
            }
            x += W; // word gap without a space glyph
        }
    }

    private static PageText TwoLines() =>
        PageText.Build(Line(0.1, 0.1, "Hello", "world").Concat(Line(0.2, 0.1, "second", "line")));

    [Fact]
    public void Text_has_spaces_between_words_and_newlines_between_lines() =>
        Assert.Equal("Hello world\nsecond line", TwoLines().GetText(0, int.MaxValue));

    [Fact]
    public void Drawn_space_glyphs_are_not_doubled()
    {
        var glyphs = new[] { "a", " ", "b" }.Select((t, i) => new Glyph(t, new TextRect(i * W, 0, (i + 1) * W, H)));
        Assert.Equal("a b", PageText.Build(glyphs).GetText(0, 3));
    }

    [Fact]
    public void Empty_glyphs_are_dropped() =>
        Assert.Equal(0, PageText.Build([new Glyph("", new TextRect(0, 0, W, H))]).Count);

    [Fact]
    public void Caret_is_before_the_glyph_whose_left_half_is_hit()
    {
        var text = TwoLines();
        Assert.Equal(0, text.CaretAt(0.1 + W * 0.4, 0.11));
        Assert.Equal(1, text.CaretAt(0.1 + W * 0.6, 0.11));
    }

    [Fact]
    public void Caret_past_a_line_end_is_the_line_end() =>
        Assert.Equal(10, TwoLines().CaretAt(0.9, 0.11)); // "Helloworld" is 10 glyphs

    [Fact]
    public void Caret_above_or_below_all_text_is_the_page_start_or_end()
    {
        var text = TwoLines();
        Assert.Equal(0, text.CaretAt(0.5, 0.01));
        Assert.Equal(text.Count, text.CaretAt(0.5, 0.9));
    }

    [Fact]
    public void Caret_between_lines_goes_to_the_nearest_line() =>
        Assert.Equal(10, TwoLines().CaretAt(0.9, 0.1 + H + 0.01)); // closer to the first line

    [Fact]
    public void Selection_is_the_same_whichever_way_it_was_dragged() =>
        Assert.Equal(TwoLines().GetText(2, 7), TwoLines().GetText(7, 2));

    [Fact]
    public void Highlight_has_one_box_per_selected_line()
    {
        var boxes = TwoLines().Highlight(3, 13).ToList();
        Assert.Equal(2, boxes.Count);
        Assert.Equal(0.1 + 3 * W, boxes[0].Left, 9);
        Assert.Equal(0.1, boxes[1].Left, 9);
        Assert.Equal(0.1 + 3 * W, boxes[1].Right, 9);
    }

    [Fact]
    public void Word_at_a_point_stops_at_word_breaks()
    {
        var text = TwoLines();
        var (start, end) = text.WordAt(0.1 + 7 * W, 0.11); // inside "world"
        Assert.Equal("world", text.GetText(start, end));
    }

    [Fact]
    public void Word_at_empty_space_is_empty()
    {
        var (start, end) = TwoLines().WordAt(0.9, 0.9);
        Assert.Equal(start, end);
    }

    [Fact]
    public void Columns_keep_their_drawing_order()
    {
        // Left column drawn fully before the right one, as most PDF producers do.
        var glyphs = Line(0.1, 0.1, "left1").Concat(Line(0.2, 0.1, "left2"))
            .Concat(Line(0.1, 0.6, "right1")).Concat(Line(0.2, 0.6, "right2"));
        Assert.Equal("left1\nleft2\nright1\nright2", PageText.Build(glyphs).GetText(0, int.MaxValue));
    }

    [Fact]
    public void Link_at_finds_the_topmost_link_under_a_point()
    {
        var under = new PageLink(new TextRect(0.1, 0.1, 0.5, 0.2), "https://a.example", null, null);
        var over = new PageLink(new TextRect(0.3, 0.1, 0.4, 0.2), null, 3, null);
        var text = PageText.Build(Line(0.1, 0.1, "hello"), [under, over]);

        Assert.Same(under, text.LinkAt(0.15, 0.15));
        Assert.Same(over, text.LinkAt(0.35, 0.15));
        Assert.Null(text.LinkAt(0.7, 0.15));
    }

    [Fact]
    public void Links_survive_a_page_without_text()
    {
        var link = new PageLink(new TextRect(0, 0, 1, 1), "https://a.example", null, null);
        Assert.Same(link, PageText.Build([], [link]).LinkAt(0.5, 0.5));
    }

    [Fact]
    public void Positions_order_by_page_then_caret()
    {
        Assert.True(new TextPosition(1, 0) > new TextPosition(0, 99));
        Assert.True(new TextPosition(2, 3) < new TextPosition(2, 4));
    }
}
