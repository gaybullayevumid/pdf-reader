namespace PdfReader.Localization;

public sealed record LanguageInfo(string Code, string NativeName, string CultureName)
{
    public override string ToString() => NativeName;
}

/// <summary>The languages the app is translated into, and how the active one is chosen.</summary>
public static class Languages
{
    public const string Fallback = "en";

    public static IReadOnlyList<LanguageInfo> All { get; } =
    [
        new("en", "English", "en-US"),
        new("uz", "O‘zbekcha", "uz-Latn-UZ"),
        new("ru", "Русский", "ru-RU"),
    ];

    public static bool IsSupported(string? code) => All.Any(l => l.Code == Normalize(code));

    /// <summary>"uz-Latn-UZ" and "RU" both map to their two-letter code; unknown values become null.</summary>
    public static string? Normalize(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        var two = code.Trim().Split('-', '_')[0].ToLowerInvariant();
        return All.Any(l => l.Code == two) ? two : null;
    }

    /// <summary>The saved choice if valid, else the system language if supported, else English.</summary>
    public static string Resolve(string? saved, string? systemCulture) =>
        Normalize(saved) ?? Normalize(systemCulture) ?? Fallback;

    public static LanguageInfo Get(string? code) =>
        All.First(l => l.Code == (Normalize(code) ?? Fallback));
}
