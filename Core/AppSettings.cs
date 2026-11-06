using System.Text.Json;
using System.Text.Json.Serialization;
using PdfReader.Models;

namespace PdfReader.Core;

/// <summary>Everything the user can configure, persisted as one JSON document.</summary>
public sealed record AppSettings(
    AppTheme Theme = AppTheme.System,
    bool AutoCheckUpdates = true,
    string? Language = null,
    bool RememberWindowState = true,
    WindowStateData? Window = null);

/// <summary>Reads and writes <see cref="AppSettings"/>; a missing or damaged file never throws.</summary>
public static class SettingsStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static AppSettings Load(string path, Action<Exception>? onError = null)
    {
        try
        {
            if (File.Exists(path))
                return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path), Options) ?? new AppSettings();
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex);
        }
        return new AppSettings();
    }

    /// <summary>Writes to a temp file first so a crash mid-write cannot leave a half-written settings file.</summary>
    public static void Save(string path, AppSettings settings)
    {
        var temp = path + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(settings, Options));
        File.Move(temp, path, overwrite: true);
    }
}
