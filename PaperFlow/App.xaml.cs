using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using PaperFlow.Database;
using PaperFlow.Services;
using PaperFlow.ViewModels;
using Serilog;

namespace PaperFlow;

public partial class App : Application
{
    private Window? _window;

    /// <summary>Application-wide service provider (composition root).</summary>
    public static IServiceProvider Services { get; private set; } = null!;

    public App()
    {
        InitializeComponent();
        ConfigureLogging();
        Services = BuildServices();
        UnhandledException += OnUnhandledException;
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        Services.GetRequiredService<AppDatabase>().Initialize();

        var window = new MainWindow();
        _window = window;
        ((WindowProvider)Services.GetRequiredService<IWindowProvider>()).Set(window);
        Services.GetRequiredService<IThemeService>().Attach(window);
        window.Activate();

        // Launched with a file (double-click / "Open with" / command line).
        var launchFiles = Environment.GetCommandLineArgs().Skip(1).ToList();
        if (launchFiles.Count > 0)
            _ = Services.GetRequiredService<IDocumentOpener>().OpenAsync(launchFiles);
    }

    private static ServiceProvider BuildServices()
    {
        var services = new ServiceCollection();

        services.AddLogging(b => b.AddSerilog(dispose: true));

        services.AddSingleton<AppDatabase>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IWindowProvider, WindowProvider>();
        services.AddSingleton<IFileService, FileService>();
        services.AddSingleton<IRecentFilesService, RecentFilesService>();
        services.AddSingleton<IDocumentOpener, DocumentOpener>();

        services.AddSingleton<MainViewModel>();
        services.AddTransient<LibraryViewModel>();
        services.AddTransient<SettingsViewModel>();

        return services.BuildServiceProvider();
    }

    private static void ConfigureLogging()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                Path.Combine(AppPaths.LogDirectory, "paperflow-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7)
            .CreateLogger();
    }

    private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unhandled exception");
        e.Handled = true;
    }
}
