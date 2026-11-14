using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using PdfReader.Localization;
using PdfReader.Models;
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

    /// <summary>Open documents; the tab bar binds to its <see cref="IDocumentTabService.Tabs"/>.</summary>
    public IDocumentTabService TabService { get; } = App.Services.GetRequiredService<IDocumentTabService>();

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

        TabService.ActiveChanged += (_, _) => ShowActiveDocument();
        TabService.Tabs.CollectionChanged += OnTabsChanged;
        Closed += (_, _) => TabService.SaveSession();
        TabService.RestoreSession(); // shows the tab that was in front last time, if any
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
    private object? _pageMenuItem; // the sidebar entry of the page in the Frame

    /// <summary>Remembers which sidebar entry belongs to the page shown in the Frame.</summary>
    private void OnFrameNavigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        _pageMenuItem = e.SourcePageType.Name switch
        {
            nameof(Views.SettingsPage) => NavView.SettingsItem,
            nameof(Views.LibraryPage) => FindMenuItem(e.Parameter?.ToString()?.ToLowerInvariant()),
            nameof(Views.HomePage) => FindMenuItem("home"),
            _ => null,
        };
        SyncSidebarSelection();
    }

    /// <summary>The sidebar shows the page's entry, or nothing while a document is in front.</summary>
    private void SyncSidebarSelection()
    {
        _syncingSelection = true;
        try { NavView.SelectedItem = TabService.ActiveTab is null ? _pageMenuItem : null; }
        finally { _syncingSelection = false; }
    }

    // ---------------------------------------------------------------- tabs

    private bool _syncingTabs;

    /// <summary>Puts the active tab's viewer in front of the page, or shows the page when no tab is active.</summary>
    private void ShowActiveDocument()
    {
        var viewer = TabService.ActiveViewer;
        DocumentHost.Child = viewer; // a hidden tab's viewer leaves the tree, so it stops reacting to input
        DocumentHost.Visibility = viewer is null ? Visibility.Collapsed : Visibility.Visible;
        ContentFrame.Visibility = viewer is null ? Visibility.Visible : Visibility.Collapsed;

        // The viewer brings its own toolbar, so the app toolbar would only waste a row of space.
        Toolbar.Visibility = viewer is null ? Visibility.Visible : Visibility.Collapsed;

        SyncTabSelection();
        SyncSidebarSelection();
    }

    private void SyncTabSelection()
    {
        _syncingTabs = true;
        try { TabBar.SelectedItem = TabService.ActiveTab; }
        finally { _syncingTabs = false; }
    }

    /// <summary>
    /// TabView moves its selection by itself when tabs are added, removed or reordered; the tab service decides
    /// what is active, so those changes are ignored and the selection is put back once TabView is done.
    /// </summary>
    private void OnTabsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        TabBar.Visibility = TabService.Tabs.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        _syncingTabs = true;
        DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, SyncTabSelection);
    }

    private void OnTabSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_syncingTabs) return;
        if (TabBar.SelectedItem is DocumentTab tab) TabService.Activate(tab);
        else SyncTabSelection();
    }

    /// <summary>A click on the selected tab while a page is in front brings the document back.</summary>
    private void OnTabTapped(object sender, TappedRoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is DocumentTab tab) TabService.Activate(tab);
    }

    private void OnTabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        if (args.Item is DocumentTab tab) TabService.Close(tab);
    }

    private void OnAddTab(TabView sender, object args) => _ = _opener.PickAndOpenAsync();

    private void OnCloseTabShortcut(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (TabService.ActiveTab is not { } tab) return;
        TabService.Close(tab);
        args.Handled = true;
    }

    private void OnNextTabShortcut(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        TabService.Cycle(1);
        args.Handled = true;
    }

    private void OnPreviousTabShortcut(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        TabService.Cycle(-1);
        args.Handled = true;
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
