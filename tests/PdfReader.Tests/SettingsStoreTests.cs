using PdfReader.Core;
using PdfReader.Models;

namespace PdfReader.Tests;

public sealed class SettingsStoreTests : IDisposable
{
    private readonly string _dir = Directory.CreateTempSubdirectory("pdfreader-tests-").FullName;
    private string FilePath => Path.Combine(_dir, "settings.json");

    public void Dispose() => Directory.Delete(_dir, recursive: true);

    [Fact]
    public void Missing_file_gives_defaults()
    {
        var settings = SettingsStore.Load(FilePath);

        Assert.Equal(new AppSettings(), settings);
        Assert.True(settings.RememberWindowState);
        Assert.True(settings.AutoCheckUpdates);
        Assert.Null(settings.Language);
        Assert.Null(settings.Window);
    }

    [Fact]
    public void Everything_survives_a_round_trip()
    {
        var saved = new AppSettings(
            Theme: AppTheme.Dark,
            AutoCheckUpdates: false,
            Language: "uz",
            RememberWindowState: false,
            Window: new WindowStateData(WindowMode.FullScreen, -20, 40, 1300, 900, WasMaximized: true));

        SettingsStore.Save(FilePath, saved);

        Assert.Equal(saved, SettingsStore.Load(FilePath));
    }

    [Fact]
    public void Language_and_window_mode_are_stored_readably()
    {
        SettingsStore.Save(FilePath, new AppSettings(Language: "ru", Window: new WindowStateData(WindowMode.Maximized)));
        var json = File.ReadAllText(FilePath);

        Assert.Contains("\"ru\"", json);
        Assert.Contains("\"Maximized\"", json);
    }

    [Fact]
    public void A_settings_file_from_an_older_version_still_loads()
    {
        File.WriteAllText(FilePath, """{"Theme":2,"AutoCheckUpdates":false}""");

        var settings = SettingsStore.Load(FilePath);

        Assert.Equal(AppTheme.Dark, settings.Theme);
        Assert.False(settings.AutoCheckUpdates);
        Assert.Null(settings.Language);
        Assert.True(settings.RememberWindowState);
    }

    [Theory]
    [InlineData("not json at all")]
    [InlineData("")]
    [InlineData("{\"Theme\": \"Sepia\"}")]
    [InlineData("[1,2,3]")]
    [InlineData("{\"Window\": {\"Mode\": 99, \"Width\": \"wide\"}}")]
    public void A_damaged_file_falls_back_to_defaults_and_reports_the_problem(string content)
    {
        File.WriteAllText(FilePath, content);
        Exception? reported = null;

        var settings = SettingsStore.Load(FilePath, ex => reported = ex);

        Assert.Equal(new AppSettings(), settings);
        Assert.NotNull(reported);
    }

    [Fact]
    public void Saving_leaves_no_temporary_file_behind()
    {
        SettingsStore.Save(FilePath, new AppSettings(Language: "en"));
        SettingsStore.Save(FilePath, new AppSettings(Language: "ru"));

        Assert.Equal(["settings.json"], Directory.GetFiles(_dir).Select(Path.GetFileName));
        Assert.Equal("ru", SettingsStore.Load(FilePath).Language);
    }

    [Fact]
    public void A_leftover_temp_file_from_a_crash_does_not_break_loading()
    {
        SettingsStore.Save(FilePath, new AppSettings(Language: "uz"));
        File.WriteAllText(FilePath + ".tmp", "{ half written");

        Assert.Equal("uz", SettingsStore.Load(FilePath).Language);

        SettingsStore.Save(FilePath, new AppSettings(Language: "ru")); // overwrites the stale temp file
        Assert.Equal("ru", SettingsStore.Load(FilePath).Language);
    }
}
