using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PdfReader.Services;

namespace PdfReader.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly IDocumentOpener _opener;
    private readonly INavigationService _navigation;
    private readonly IUpdateService _updates;
    private readonly IWindowStateService _window;
    private readonly ILocalizationService _loc;

    public MainViewModel(
        IDocumentOpener opener,
        INavigationService navigation,
        IUpdateService updates,
        IWindowStateService window,
        ILocalizationService loc)
    {
        _opener = opener;
        _navigation = navigation;
        _updates = updates;
        _window = window;
        _loc = loc;

        _updates.AvailableChanged += (_, _) => ShowUpdate();
        _loc.LanguageChanged += (_, _) =>
        {
            if (IsUpdateOpen && !IsUpdating) ShowUpdate();
        };
        _opener.Notification += (_, message) =>
        {
            StatusMessage = message;
            IsStatusOpen = true;
        };
    }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsStatusOpen { get; set; }

    [ObservableProperty]
    public partial string UpdateMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsUpdateOpen { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(InstallUpdateCommand))]
    public partial bool IsUpdating { get; set; }

    private void ShowUpdate()
    {
        if (_updates.Available is { } update)
        {
            UpdateMessage = _loc.Format("update.bar.message", update.Version, _updates.CurrentVersion);
            IsUpdateOpen = true;
        }
        else
        {
            IsUpdateOpen = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanInstallUpdate))]
    private async Task InstallUpdateAsync()
    {
        IsUpdating = true;
        UpdateMessage = _loc.Get("update.downloading");
        try
        {
            await _updates.InstallAsync(new Progress<double>(p => UpdateMessage = _loc.Format("update.downloadingPct", p.ToString("P0", _loc.Culture))));
        }
        catch (Exception ex)
        {
            UpdateMessage = _loc.Format("update.failed", ex.Message);
            IsUpdating = false;
        }
    }

    private bool CanInstallUpdate() => !IsUpdating;

    [RelayCommand]
    private Task OpenPdfAsync() => _opener.PickAndOpenAsync();

    [RelayCommand]
    private void ToggleFullScreen() => _window.ToggleFullScreen();

    public void Navigate(string tag) => _navigation.NavigateTo(tag);
}
