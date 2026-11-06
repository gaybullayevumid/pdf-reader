using Microsoft.Extensions.Logging;
using PdfReader.Core;
using PdfReader.Database;
using PdfReader.Models;

namespace PdfReader.Services;

public interface ISettingsService
{
    AppTheme Theme { get; set; }
    bool AutoCheckUpdates { get; set; }

    /// <summary>Chosen UI language code, or null until the user picks one (then the system language is used).</summary>
    string? Language { get; set; }

    bool RememberWindowState { get; set; }

    /// <summary>Last saved window state; null when none has been saved yet.</summary>
    WindowStateData? Window { get; set; }
}

/// <summary>Settings persisted as JSON in %LocalAppData%\PdfReader (works packaged and unpackaged).</summary>
public sealed class SettingsService : ISettingsService
{
    private static readonly string FilePath = Path.Combine(AppPaths.DataDirectory, "settings.json");
    private readonly ILogger<SettingsService> _logger;
    private readonly object _gate = new();
    private AppSettings _data;

    public SettingsService(ILogger<SettingsService> logger)
    {
        _logger = logger;
        _data = SettingsStore.Load(FilePath, ex => logger.LogWarning(ex, "Could not read settings; using defaults"));
    }

    public AppTheme Theme
    {
        get => _data.Theme;
        set => Update(d => d with { Theme = value });
    }

    public bool AutoCheckUpdates
    {
        get => _data.AutoCheckUpdates;
        set => Update(d => d with { AutoCheckUpdates = value });
    }

    public string? Language
    {
        get => _data.Language;
        set => Update(d => d with { Language = value });
    }

    public bool RememberWindowState
    {
        get => _data.RememberWindowState;
        set => Update(d => d with { RememberWindowState = value });
    }

    public WindowStateData? Window
    {
        get => _data.Window;
        set => Update(d => d with { Window = value });
    }

    private void Update(Func<AppSettings, AppSettings> change)
    {
        lock (_gate)
        {
            _data = change(_data);
            try
            {
                SettingsStore.Save(FilePath, _data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not save settings");
            }
        }
    }
}
