using System.Globalization;
using Microsoft.Extensions.Logging;
using PdfReader.Localization;

namespace PdfReader.Services;

public interface ILocalizationService
{
    /// <summary>Two-letter code of the active language ("en", "uz", "ru").</summary>
    string Language { get; }

    CultureInfo Culture { get; }

    event EventHandler? LanguageChanged;

    /// <summary>Switches language, saves the choice and notifies every bound element.</summary>
    void SetLanguage(string code);

    string Get(string key);

    string Format(string key, params object?[] args);
}

public sealed class LocalizationService : ILocalizationService
{
    private readonly StringCatalog _catalog = StringCatalog.LoadFromAssembly(typeof(LocalizationService).Assembly);
    private readonly ISettingsService _settings;
    private readonly ILogger<LocalizationService> _logger;

    public LocalizationService(ISettingsService settings, ILogger<LocalizationService> logger)
    {
        _settings = settings;
        _logger = logger;
        Language = Languages.Resolve(settings.Language, CultureInfo.CurrentUICulture.Name);
        Culture = ApplyCulture(Language);
    }

    public string Language { get; private set; }

    public CultureInfo Culture { get; private set; }

    public event EventHandler? LanguageChanged;

    public void SetLanguage(string code)
    {
        var normalized = Languages.Normalize(code) ?? Languages.Fallback;
        if (normalized == Language && _settings.Language == normalized) return;

        _settings.Language = normalized;
        if (normalized == Language) return;

        Language = normalized;
        Culture = ApplyCulture(normalized);
        _logger.LogInformation("Language changed to {Language}", normalized);
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    public string Get(string key) => _catalog.Get(Language, key);

    public string Format(string key, params object?[] args) => _catalog.Format(Language, key, args);

    private static CultureInfo ApplyCulture(string language)
    {
        var culture = CultureInfo.GetCultureInfo(Languages.Get(language).CultureName);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        return culture;
    }
}
