using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using PdfReader.Localization;
using PdfReader.Services;
using PdfReader.ViewModels;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace PdfReader;

public sealed partial class MainWindow : Window
{
    private readonly IDocumentOpener _opener = App.Services.GetRequiredService<IDocumentOpener>();
    private readonly IWindowStateService _windowState = App.Services.GetRequiredService<IWindowStateService>();

    public MainViewModel ViewModel { get; } = App.Services.GetRequiredService<MainViewModel>();

    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppWindow.SetIcon("Assets/AppIcon.ico");

        // The NavigationView's built-in Settings entry only exists once its template is applied.
        NavView.Loaded += (_, _) => LocalizeSettingsItem();

        _windowState.Attach(this);
        _windowState.RestoreBounds();

        App.Services.GetRequiredService<INavigationService>().Attach(ContentFrame);
        ContentFrame.Navigated += OnFrameNavigated;
        ViewModel.Navigate("home");
    }

    /// <summary>The built-in Settings entry has its own OS-language caption and accessible name; use ours.</summary>
    private void LocalizeSettingsItem()
    {
        if (NavView.SettingsItem is not NavigationViewItem item) return;
        Loc.SetContent(item, "nav.settings");
        Loc.SetAutomationName(item, "nav.settings");
    }

    private void OnPaneToggleRequested(TitleBar sender, object args) =>
        NavView.IsPaneOpen = !NavView.IsPaneOpen;

    private void OnToggleFullScreen(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        _windowState.ToggleFullScreen();
        args.Handled = true;
    }

    private void OnEscape(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (!_windowState.IsFullScreen) return;
        _windowState.SetFullScreen(false);
        args.Handled = true;
    }

    private bool _syncingSelection;

    /// <summary>Keeps the sidebar selection in step with the page shown (e.g. none while reading a PDF).</summary>
    private void OnFrameNavigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        object? target = e.SourcePageType.Name switch
        {
            nameof(Views.SettingsPage) => NavView.SettingsItem,
            nameof(Views.LibraryPage) => FindMenuItem(e.Parameter?.ToString()?.ToLowerInvariant()),
            nameof(Views.HomePage) => FindMenuItem("home"),
            _ => null,
        };
        _syncingSelection = true;
        try { NavView.SelectedItem = target; }
        finally { _syncingSelection = false; }

        // The viewer brings its own toolbar, so the app toolbar would only waste a row of space.
        Toolbar.Visibility = e.SourcePageType == typeof(Views.ViewerPage) ? Visibility.Collapsed : Visibility.Visible;
    }

    private NavigationViewItem? FindMenuItem(string? tag) =>
        NavView.MenuItems.OfType<NavigationViewItem>().FirstOrDefault(i => (i.Tag as string) == tag);

    private void OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (_syncingSelection || args.SelectedItem is null) return;
        var tag = args.IsSettingsSelected ? "settings" : (args.SelectedItemContainer?.Tag as string ?? "home");
        ViewModel.Navigate(tag);
    }

    private void OnOpenSettings(object sender, RoutedEventArgs e) =>
        NavView.SelectedItem = NavView.SettingsItem;

    private void OnDragOver(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.Caption = Loc.Get("toolbar.open");
        }
    }

    private async void OnDrop(object sender, DragEventArgs e)
    {
        if (!e.DataView.Contains(StandardDataFormats.StorageItems)) return;

        var deferral = e.GetDeferral();
        try
        {
            var items = await e.DataView.GetStorageItemsAsync();
            await _opener.OpenAsync(items.OfType<StorageFile>().Select(f => f.Path));
        }
        finally
        {
            deferral.Complete();
        }
    }
}
