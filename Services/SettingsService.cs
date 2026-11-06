using System.Text.Json;
using Microsoft.Extensions.Logging;
using PdfReader.Database;
using PdfReader.Models;

namespace PdfReader.Services;

public interface ISettingsService
{
    AppTheme Theme { get; set; }
}

/// <summary>Settings persisted as JSON in %LocalAppData%\PdfReader (works packaged and unpackaged).</summary>
public sealed class SettingsService : ISettingsService
{
    private sealed record Data(AppTheme Theme = AppTheme.System);

    private static readonly string FilePath = Path.Combine(AppPaths.DataDirectory, "settings.json");
    private readonly ILogger<SettingsService> _logger;
    private Data _data;

    public SettingsService(ILogger<SettingsService> logger)
    {
        _logger = logger;
        _data = Load();
    }

    public AppTheme Theme
    {
        get => _data.Theme;
        set
        {
            _data = _data with { Theme = value };
            Save();
        }
    }

    private Data Load()
    {
        try
        {
            if (File.Exists(FilePath))
                return JsonSerializer.Deserialize<Data>(File.ReadAllText(FilePath)) ?? new Data();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not read settings; using defaults");
        }
        return new Data();
    }

    private void Save()
    {
        try
        {
            File.WriteAllText(FilePath, JsonSerializer.Serialize(_data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not save settings");
        }
    }
}
