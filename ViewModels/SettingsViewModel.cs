using CommunityToolkit.Mvvm.ComponentModel;
using PdfReader.Models;
using PdfReader.Services;

namespace PdfReader.ViewModels;

public sealed partial class SettingsViewModel(IThemeService themes) : ObservableObject
{
    /// <summary>0 = System, 1 = Light, 2 = Dark (matches <see cref="AppTheme"/>).</summary>
    [ObservableProperty]
    public partial int ThemeIndex { get; set; } = (int)themes.Current;

    partial void OnThemeIndexChanged(int value)
    {
        if (value is >= 0 and <= 2) themes.Apply((AppTheme)value);
    }
}
