using PdfReader.Core;

namespace PdfReader.Tests;

public class TabRulesTests
{
    [Theory]
    [InlineData(2, 2, 5, 2)] // middle active tab closed: its right neighbour takes its place
    [InlineData(4, 4, 5, 3)] // last active tab closed: the left neighbour
    [InlineData(0, 3, 5, 2)] // a tab left of the active one closed: the active tab shifts left
    [InlineData(4, 1, 5, 1)] // a tab right of the active one closed: nothing moves
    [InlineData(0, 0, 1, -1)] // the only tab closed
    [InlineData(1, -1, 3, -1)] // no tab active (a library page is showing): none becomes active
    public void Active_after_close_follows_browser_behaviour(int closed, int active, int count, int expected) =>
        Assert.Equal(expected, TabRules.ActiveAfterClose(closed, active, count));

    [Fact]
    public void Same_file_is_found_whatever_the_case_or_spelling()
    {
        var dir = Path.GetTempPath();
        var paths = new[] { Path.Combine(dir, "a.pdf"), Path.Combine(dir, "Book.pdf") };
        Assert.Equal(1, TabRules.IndexOf(paths, Path.Combine(dir, "BOOK.PDF")));
        Assert.Equal(1, TabRules.IndexOf(paths, Path.Combine(dir, "sub", "..", "Book.pdf")));
        Assert.Equal(-1, TabRules.IndexOf(paths, Path.Combine(dir, "other.pdf")));
    }
}

public sealed class SessionStoreTests : IDisposable
{
    private readonly string _dir = Directory.CreateTempSubdirectory("pdfreader-session-").FullName;
    private string File(string name = "session.json") => Path.Combine(_dir, name);

    public void Dispose() => Directory.Delete(_dir, recursive: true);

    [Fact]
    public void Tabs_and_their_positions_round_trip()
    {
        var session = new SessionData(
        [
            new TabEntry(@"C:\docs\a.pdf", new ViewState(12, 0.25, 1.5, FitWidth: false, HorizontalOffset: 40)),
            new TabEntry(@"C:\docs\b.pdf"),
        ], Active: 1);

        SessionStore.Save(File(), session);
        var loaded = SessionStore.Load(File());

        Assert.Equal(1, loaded.Active);
        Assert.Equal(session.Tabs, loaded.Tabs);
    }

    [Fact]
    public void Missing_file_is_an_empty_session()
    {
        var loaded = SessionStore.Load(File("none.json"));
        Assert.Empty(loaded.Tabs);
        Assert.Equal(-1, loaded.Active);
    }

    [Fact]
    public void Damaged_file_is_an_empty_session_and_reports_the_error()
    {
        System.IO.File.WriteAllText(File(), "{ not json");
        Exception? error = null;
        var loaded = SessionStore.Load(File(), ex => error = ex);
        Assert.Empty(loaded.Tabs);
        Assert.NotNull(error);
    }

    [Fact]
    public void Out_of_range_active_index_and_blank_paths_are_dropped()
    {
        System.IO.File.WriteAllText(File(), """{ "Tabs": [ { "Path": "" }, { "Path": "C:\\x.pdf" } ], "Active": 7 }""");
        var loaded = SessionStore.Load(File());
        Assert.Single(loaded.Tabs);
        Assert.Equal(-1, loaded.Active);
    }
}
