using System.Text.Json;
using PdfReader.Core;

namespace PdfReader.Tests;

public class ReleaseParserTests
{
    private const string Sha = "1cf9bea5e483fdd2501a87bfa64ce0eb35e747d9dabeb5ebe9473260b912d26e";

    private static JsonElement Release(string tag = "v1.3.0", bool draft = false, bool prerelease = false, string? assets = null)
    {
        assets ??= $$"""
            [
              {"name":"PDFReader-1.3.0-win-x64-portable.zip","browser_download_url":"https://github.com/o/r/releases/download/{{tag}}/portable.zip"},
              {"name":"PDFReader-Setup-1.3.0.exe","browser_download_url":"https://github.com/o/r/releases/download/{{tag}}/PDFReader-Setup-1.3.0.exe","digest":"sha256:{{Sha}}"}
            ]
            """;
        var json = $$"""{"tag_name":"{{tag}}","draft":{{draft.ToString().ToLowerInvariant()}},"prerelease":{{prerelease.ToString().ToLowerInvariant()}},"html_url":"https://github.com/o/r/releases/tag/{{tag}}","assets":{{assets}}}""";
        return JsonDocument.Parse(json).RootElement.Clone();
    }

    private static bool TrustGitHub(string url) => url.StartsWith("https://github.com/");

    [Fact]
    public void Picks_the_installer_and_ignores_other_assets()
    {
        var update = ReleaseParser.Parse(Release(), TrustGitHub);

        Assert.NotNull(update);
        Assert.Equal(new Version(1, 3, 0), update.Version);
        Assert.Equal("PDFReader-Setup-1.3.0.exe", update.InstallerName);
        Assert.EndsWith("PDFReader-Setup-1.3.0.exe", update.InstallerUrl);
        Assert.Equal(Sha, update.Sha256);
        Assert.Contains("/releases/tag/v1.3.0", update.PageUrl);
    }

    [Fact]
    public void Draft_and_prerelease_are_ignored()
    {
        Assert.Null(ReleaseParser.Parse(Release(draft: true), TrustGitHub));
        Assert.Null(ReleaseParser.Parse(Release(prerelease: true), TrustGitHub));
    }

    [Fact]
    public void A_release_without_an_installer_is_ignored()
    {
        var assets = """[{"name":"source.zip","browser_download_url":"https://github.com/o/r/a.zip"}]""";
        Assert.Null(ReleaseParser.Parse(Release(assets: assets), TrustGitHub));
    }

    [Fact]
    public void An_installer_from_an_untrusted_host_is_never_offered()
    {
        var assets = """[{"name":"PDFReader-Setup-9.9.9.exe","browser_download_url":"https://evil.example/PDFReader-Setup-9.9.9.exe"}]""";
        Assert.Null(ReleaseParser.Parse(Release(assets: assets), TrustGitHub));
    }

    [Fact]
    public void An_installer_without_a_checksum_is_still_offered()
    {
        var assets = """[{"name":"PDFReader-Setup-1.3.0.exe","browser_download_url":"https://github.com/o/r/a.exe"}]""";
        var update = ReleaseParser.Parse(Release(assets: assets), TrustGitHub);

        Assert.NotNull(update);
        Assert.Null(update.Sha256);
    }

    [Theory]
    [InlineData("nightly")]
    [InlineData("")]
    [InlineData("release-candidate")]
    public void A_tag_that_is_not_a_version_is_ignored(string tag) =>
        Assert.Null(ReleaseParser.Parse(Release(tag: tag), TrustGitHub));

    [Fact]
    public void Missing_fields_do_not_throw()
    {
        Assert.Null(ReleaseParser.Parse(JsonDocument.Parse("{}").RootElement, TrustGitHub));
        Assert.Null(ReleaseParser.Parse(JsonDocument.Parse("""{"tag_name":"v2.0.0"}""").RootElement, TrustGitHub));
        Assert.Null(ReleaseParser.Parse(JsonDocument.Parse("""{"tag_name":"v2.0.0","assets":[{}]}""").RootElement, TrustGitHub));
    }

    [Theory]
    [InlineData("v1.2.0", 1, 2, 0)]
    [InlineData("V2.0.1", 2, 0, 1)]
    [InlineData("1.10", 1, 10, 0)]
    [InlineData(" v3.4.5 ", 3, 4, 5)]
    [InlineData("v1.2.0.0", 1, 2, 0)]
    public void Tags_parse_to_versions(string tag, int major, int minor, int build)
    {
        Assert.True(ReleaseParser.TryParseVersion(tag, out var version));
        Assert.Equal(new Version(major, minor, build), version);
    }

    [Theory]
    [InlineData("1.3.0", "1.2.0")]
    [InlineData("1.10.0", "1.9.9")]
    [InlineData("2.0.0", "1.99.99")]
    public void Newer_versions_compare_greater(string newer, string older)
    {
        ReleaseParser.TryParseVersion(newer, out var a);
        ReleaseParser.TryParseVersion(older, out var b);
        Assert.True(a > b);
    }

    [Fact]
    public void The_assembly_style_four_part_version_equals_the_tag_version()
    {
        ReleaseParser.TryParseVersion("v1.2.0", out var tag);
        Assert.Equal(tag, ReleaseParser.Normalize(new Version(1, 2, 0, 0)));
        Assert.Equal(new Version(0, 0, 0), ReleaseParser.Normalize(null));
    }
}
