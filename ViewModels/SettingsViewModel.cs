using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PdfReader.Localization;
using PdfReader.Models;
using PdfReader.Services;

namespace PdfReader.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    private enum UpdateState { Idle, Checking, UpToDate, Available, Downloading, CheckFailed, Failed }

    private readonly IThemeService _themes;
    private readonly ISettingsService _settings;
    private readonly IUpdateService _updates;
    private readonly ILocalizationService _loc;
    private readonly IWindowStateService _window;

    private UpdateState _state = UpdateState.Idle;
    private string _stateDetail = string.Empty;
    private bool _syncing;

    public SettingsViewModel(
        IThemeService themes,
        ISettingsService settings,
        IUpdateService updates,
        ILocalizationService loc,
        IWindowStateService window)
    {
        _themes = themes;
        _settings = settings;
        _updates = updates;
        _loc = loc;
        _window = window;

        ThemeIndex = (int)themes.Current;
        AutoCheckUpdates = settings.AutoCheckUpdates;
        RememberWindowState = settings.RememberWindowState;
        IsFullScreen = window.IsFullScreen;
        SelectedLanguage = Languages.Get(loc.Language);

        if (updates.Available is not null) _state = UpdateState.Available;
        RefreshUpdateTexts();

        _loc.LanguageChanged += OnLanguageChanged;
        _window.FullScreenChanged += OnFullScreenChanged;
    }

    /// <summary>Unsubscribes from app-wide events; called when the page is left.</summary>
    public void Detach()
    {
        _loc.LanguageChanged -= OnLanguageChanged;
        _window.FullScreenChanged -= OnFullScreenChanged;
    }

    // ---- appearance ---------------------------------------------------------------------------

    /// <summary>0 = System, 1 = Light, 2 = Dark (matches <see cref="AppTheme"/>).</summary>
    [ObservableProperty]
    public partial int ThemeIndex { get; set; }

    partial void OnThemeIndexChanged(int value)
    {
        if (value is >= 0 and <= 2) _themes.Apply((AppTheme)value);
    }

    // ---- language -----------------------------------------------------------------------------

    public IReadOnlyList<LanguageInfo> AvailableLanguages => Languages.All;

    [ObservableProperty]
    public partial LanguageInfo SelectedLanguage { get; set; }

    partial void OnSelectedLanguageChanged(LanguageInfo value)
    {
        if (value is not null) _loc.SetLanguage(value.Code);
    }

    // ---- window & display ---------------------------------------------------------------------

    [ObservableProperty]
    public partial bool IsFullScreen { get; set; }

    partial void OnIsFullScreenChanged(bool value)
    {
        if (!_syncing) _window.SetFullScreen(value);
    }

    [ObservableProperty]
    public partial bool RememberWindowState { get; set; }

    partial void OnRememberWindowStateChanged(bool value)
    {
        _settings.RememberWindowState = value;
        if (value) _window.Flush();
    }

    [RelayCommand]
    private void ResetWindow() => _window.ResetLayout();

    private void OnFullScreenChanged(object? sender, EventArgs e)
    {
        _syncing = true;
        try { IsFullScreen = _window.IsFullScreen; }
        finally { _syncing = false; }
    }

    // ---- about & updates ----------------------------------------------------------------------

    [ObservableProperty]
    public partial bool AutoCheckUpdates { get; set; }

    partial void OnAutoCheckUpdatesChanged(bool value) => _settings.AutoCheckUpdates = value;

    [ObservableProperty]
    public partial string VersionText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string UpdateStatus { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string InstallLabel { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CheckCommand), nameof(InstallCommand))]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(InstallCommand))]
    public partial bool CanInstall { get; set; }

    [RelayCommand(CanExecute = nameof(CanRun))]
    private async Task CheckAsync()
    {
        IsBusy = true;
        SetState(UpdateState.Checking);
        try
        {
            SetState((await _updates.CheckAsync()) switch
            {
                UpdateCheckResult.Available => UpdateState.Available,
                UpdateCheckResult.UpToDate => UpdateState.UpToDate,
                _ => UpdateState.CheckFailed,
            });
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanInstallNow))]
    private async Task InstallAsync()
    {
        IsBusy = true;
        SetState(UpdateState.Downloading, _loc.Get("update.downloading"));
        try
        {
            await _updates.InstallAsync(new Progress<double>(p =>
                SetState(UpdateState.Downloading, _loc.Format("update.downloadingPct", p.ToString("P0", _loc.Culture)))));
        }
        catch (Exception ex)
        {
            SetState(UpdateState.Failed, ex.Message);
            IsBusy = false;
        }
    }

    private bool CanRun() => !IsBusy;
    private bool CanInstallNow() => CanInstall && !IsBusy;

    private void SetState(UpdateState state, string detail = "")
    {
        _state = state;
        _stateDetail = detail;
        RefreshUpdateTexts();
    }

    /// <summary>Rebuilds every update-related string from the current state, so a language switch re-translates them.</summary>
    private void RefreshUpdateTexts()
    {
        VersionText = _loc.Format("about.version", _updates.CurrentVersion);
        CanInstall = _state is UpdateState.Available;
        InstallLabel = _updates.Available is { } available
            ? _loc.Format("update.install", available.Version)
            : _loc.Get("update.check");

        UpdateStatus = _state switch
        {
            UpdateState.Checking => _loc.Get("update.checking"),
            UpdateState.UpToDate => _loc.Get("update.latest"),
            UpdateState.Available => _loc.Format("update.available", _updates.Available?.Version),
            UpdateState.Downloading => _stateDetail,
            UpdateState.CheckFailed => _loc.Get("update.checkFailed"),
            UpdateState.Failed => _loc.Format("update.failed", _stateDetail),
            _ => _loc.Get("update.hint"),
        };
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        RefreshUpdateTexts();
    }
}
