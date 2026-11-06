using System.Text.RegularExpressions;
using PdfReader.Localization;

namespace PdfReader.Tests;

public class LanguagesTests
{
    [Theory]
    [InlineData("uz", null, "uz")]
    [InlineData("ru", "uz-Latn-UZ", "ru")]
    [InlineData(null, "ru-RU", "ru")]
    [InlineData(null, "uz-Cyrl-UZ", "uz")]
    [InlineData(null, "UZ", "uz")]
    [InlineData(null, "en-GB", "en")]
    [InlineData(null, "de-DE", "en")]
    [InlineData("xx", "de-DE", "en")]
    [InlineData("", "", "en")]
    [InlineData(null, null, "en")]
    public void Resolve_picks_saved_then_system_then_english(string? saved, string? system, string expected) =>
        Assert.Equal(expected, Languages.Resolve(saved, system));

    [Fact]
    public void All_contains_the_three_supported_languages() =>
        Assert.Equal(["en", "uz", "ru"], Languages.All.Select(l => l.Code));

    [Fact]
    public void Native_names_are_shown_in_their_own_language()
    {
        Assert.Equal("English", Languages.Get("en").NativeName);
        Assert.Equal("O‘zbekcha", Languages.Get("uz").NativeName);
        Assert.Equal("Русский", Languages.Get("ru").NativeName);
    }

    [Fact]
    public void Get_falls_back_to_english_for_unknown_codes() =>
        Assert.Equal("en", Languages.Get("zz").Code);
}

public class StringCatalogTests
{
    private static readonly StringCatalog Catalog = StringCatalog.LoadFromAssembly(typeof(StringCatalogTests).Assembly);
    private static readonly Regex Placeholder = new(@"\{\d+\}", RegexOptions.Compiled);

    public static TheoryData<string> Translated => new() { "uz", "ru" };

    [Theory]
    [MemberData(nameof(Translated))]
    public void Every_english_key_is_translated(string language)
    {
        var missing = Catalog.Keys("en").Where(k => !Catalog.Contains(language, k)).ToList();
        Assert.True(missing.Count == 0, $"{language} is missing: {string.Join(", ", missing)}");
    }

    [Theory]
    [MemberData(nameof(Translated))]
    public void No_translation_has_keys_english_lacks(string language)
    {
        var extra = Catalog.Keys(language).Where(k => !Catalog.Contains("en", k)).ToList();
        Assert.True(extra.Count == 0, $"{language} has unknown keys: {string.Join(", ", extra)}");
    }

    [Theory]
    [InlineData("en")]
    [InlineData("uz")]
    [InlineData("ru")]
    public void No_string_is_blank(string language)
    {
        var blank = Catalog.Keys(language).Where(k => string.IsNullOrWhiteSpace(Catalog.Get(language, k))).ToList();
        Assert.True(blank.Count == 0, $"{language} has blank strings: {string.Join(", ", blank)}");
    }

    [Theory]
    [MemberData(nameof(Translated))]
    public void Placeholders_match_english(string language)
    {
        foreach (var key in Catalog.Keys("en"))
        {
            var expected = Placeholder.Matches(Catalog.Get("en", key)).Select(m => m.Value).OrderBy(v => v);
            var actual = Placeholder.Matches(Catalog.Get(language, key)).Select(m => m.Value).OrderBy(v => v);
            Assert.True(expected.SequenceEqual(actual), $"{language}/{key}: placeholders differ from English");
        }
    }

    [Theory]
    [MemberData(nameof(Translated))]
    public void Strings_are_actually_translated(string language)
    {
        // Units are allowed to look the same in Uzbek (B, KB, MB).
        var allowedSame = language == "uz" ? new[] { "size.b", "size.kb", "size.mb" } : [];
        var untranslated = Catalog.Keys("en")
            .Where(k => !allowedSame.Contains(k))
            .Where(k => Catalog.Get("en", k) == Catalog.Get(language, k))
            .ToList();
        Assert.True(untranslated.Count == 0, $"{language} still has English text for: {string.Join(", ", untranslated)}");
    }

    [Fact]
    public void Russian_is_cyrillic_and_uzbek_is_latin()
    {
        var cyrillic = new Regex(@"\p{IsCyrillic}");
        Assert.All(Catalog.Keys("ru"), k => Assert.Matches(cyrillic, Catalog.Get("ru", k)));
        Assert.All(Catalog.Keys("uz"), k => Assert.DoesNotMatch(cyrillic, Catalog.Get("uz", k)));
    }

    [Fact]
    public void Unknown_key_falls_back_to_the_key_itself() =>
        Assert.Equal("no.such.key", Catalog.Get("uz", "no.such.key"));

    [Fact]
    public void Missing_translation_falls_back_to_english()
    {
        var catalog = new StringCatalog(new Dictionary<string, IReadOnlyDictionary<string, string>>
        {
            ["en"] = new Dictionary<string, string> { ["a"] = "Hello" },
            ["uz"] = new Dictionary<string, string>(),
        });
        Assert.Equal("Hello", catalog.Get("uz", "a"));
    }

    [Fact]
    public void Format_inserts_arguments_in_each_language()
    {
        Assert.Equal("Version 1.2.0", Catalog.Format("en", "about.version", "1.2.0"));
        Assert.Equal("Versiya 1.2.0", Catalog.Format("uz", "about.version", "1.2.0"));
        Assert.Equal("Версия 1.2.0", Catalog.Format("ru", "about.version", "1.2.0"));
    }

    [Fact]
    public void Format_survives_a_broken_template()
    {
        var catalog = new StringCatalog(new Dictionary<string, IReadOnlyDictionary<string, string>>
        {
            ["en"] = new Dictionary<string, string> { ["bad"] = "oops {" },
        });
        Assert.Equal("oops {", catalog.Format("en", "bad", 1));
    }
}

/// <summary>Makes sure every key referenced from XAML or C# exists, and that no key is left unused.</summary>
public class KeyUsageTests
{
    private static readonly StringCatalog Catalog = StringCatalog.LoadFromAssembly(typeof(KeyUsageTests).Assembly);

    private static readonly Regex[] KeyPatterns =
    [
        new(@"loc:Loc\.\w+=""([\w.]+)"""),
        new(@"\b(?:TitleKey|DescriptionKey)=""([\w.]+)"""),
        new(@"\b(?:Loc|loc|_loc|Service)\.(?:Get|Format)\(\s*""([\w.]+)"""),
        new(@"Loc\.Set\w+\([^,]+,\s*""([\w.]+)"""),
        new(@"Loc\.Get\(\s*\w+\s*\?\s*""([\w.]+)""\s*:\s*""([\w.]+)"""),
        new(@"\?\s*""([\w.]+)""\s*:\s*""([\w.]+)"""),
    ];

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "PdfReader.csproj"))) dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }

    private static IEnumerable<string> SourceFiles()
    {
        var sep = Path.DirectorySeparatorChar;
        return Directory.EnumerateFiles(RepoRoot(), "*.*", SearchOption.AllDirectories)
            .Where(f => (f.EndsWith(".xaml") || f.EndsWith(".cs")) &&
                        !f.Contains($"{sep}obj{sep}") && !f.Contains($"{sep}bin{sep}") &&
                        !f.Contains($"{sep}tests{sep}") && !f.Contains($"{sep}installer{sep}"));
    }

    private static HashSet<string> UsedKeys()
    {
        var keys = new HashSet<string>();
        foreach (var file in SourceFiles())
        {
            var text = File.ReadAllText(file);
            foreach (var pattern in KeyPatterns)
                foreach (Match match in pattern.Matches(text))
                    foreach (var group in match.Groups.Cast<Group>().Skip(1))
                        if (group.Success && group.Value.Contains('.') && Catalog.Contains("en", group.Value)) keys.Add(group.Value);
        }
        return keys;
    }

    private static HashSet<string> ReferencedKeysIncludingUnknown()
    {
        var keys = new HashSet<string>();
        foreach (var file in SourceFiles())
        {
            var text = File.ReadAllText(file);
            foreach (var pattern in KeyPatterns.Take(4))
                foreach (Match match in pattern.Matches(text))
                    foreach (var group in match.Groups.Cast<Group>().Skip(1))
                        if (group.Success) keys.Add(group.Value);
        }
        return keys;
    }

    [Fact]
    public void Every_referenced_key_exists()
    {
        var referenced = ReferencedKeysIncludingUnknown();
        Assert.NotEmpty(referenced);
        var unknown = referenced.Where(k => !Catalog.Contains("en", k)).OrderBy(k => k).ToList();
        Assert.True(unknown.Count == 0, $"Keys used in code but missing from the strings: {string.Join(", ", unknown)}");
    }

    [Fact]
    public void No_key_is_unused()
    {
        var used = UsedKeys();
        // Library texts are built from "lib.<filter>.<part>" at runtime.
        var unused = Catalog.Keys("en")
            .Where(k => !k.StartsWith("lib.") && !used.Contains(k))
            .OrderBy(k => k).ToList();
        Assert.True(unused.Count == 0, $"Keys never referenced: {string.Join(", ", unused)}");
    }

    [Fact]
    public void Library_keys_exist_for_every_filter()
    {
        foreach (var filter in new[] { "recent", "all", "favorites", "bookmarks" })
            foreach (var part in new[] { "title", "emptyTitle", "emptyDesc" })
                Assert.True(Catalog.Contains("en", $"lib.{filter}.{part}"), $"lib.{filter}.{part}");
    }
}
