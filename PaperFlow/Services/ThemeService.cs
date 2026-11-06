using Microsoft.UI.Xaml;
using PaperFlow.Models;

namespace PaperFlow.Services;

public interface IThemeService
{
    AppTheme Current { get; }
    void Attach(Window window);
    void Apply(AppTheme theme);
}

public sealed class ThemeService(ISettingsService settings) : IThemeService
{
    private Window? _window;

    public AppTheme Current => settings.Theme;

    public void Attach(Window window)
    {
        _window = window;
        ApplyToWindow(settings.Theme);
    }

    public void Apply(AppTheme theme)
    {
        settings.Theme = theme;
        ApplyToWindow(theme);
    }

    private void ApplyToWindow(AppTheme theme)
    {
        if (_window?.Content is FrameworkElement root)
        {
            root.RequestedTheme = theme switch
            {
                AppTheme.Light => ElementTheme.Light,
                AppTheme.Dark => ElementTheme.Dark,
                _ => ElementTheme.Default,
            };
        }
    }
}
