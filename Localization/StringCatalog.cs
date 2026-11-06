using System.Reflection;
using System.Text.Json;

namespace PdfReader.Localization;

/// <summary>
/// Translated UI strings, loaded from the embedded <c>Strings.&lt;code&gt;.json</c> files.
/// A missing key falls back to English, then to the key itself, so a gap never shows an empty label.
/// </summary>
public sealed class StringCatalog
{
    private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> _byLanguage;

    public StringCatalog(IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> byLanguage) =>
        _byLanguage = byLanguage;

    public static StringCatalog LoadFromAssembly(Assembly assembly)
    {
        var loaded = new Dictionary<string, IReadOnlyDictionary<string, string>>();
        foreach (var language in Languages.All)
        {
            using var stream = assembly.GetManifestResourceStream($"Strings.{language.Code}.json")
                ?? throw new InvalidOperationException($"Missing embedded strings for '{language.Code}'.");
            loaded[language.Code] = Parse(stream);
        }
        return new StringCatalog(loaded);
    }

    public static IReadOnlyDictionary<string, string> Parse(Stream json) =>
        JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];

    public IReadOnlyCollection<string> Keys(string language) =>
        _byLanguage.TryGetValue(language, out var strings) ? strings.Keys.ToList() : [];

    public bool Contains(string language, string key) =>
        _byLanguage.TryGetValue(language, out var strings) && strings.ContainsKey(key);

    public string Get(string language, string key)
    {
        if (_byLanguage.TryGetValue(language, out var strings) && strings.TryGetValue(key, out var value)) return value;
        if (_byLanguage.TryGetValue(Languages.Fallback, out var english) && english.TryGetValue(key, out value)) return value;
        return key;
    }

    public string Format(string language, string key, params object?[] args)
    {
        var template = Get(language, key);
        try { return string.Format(template, args); }
        catch (FormatException) { return template; }
    }
}
