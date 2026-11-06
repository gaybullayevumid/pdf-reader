using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PaperFlow.Services;
using PaperFlow.ViewModels;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace PaperFlow;

public sealed partial class MainWindow : Window
{
    private readonly IDocumentOpener _opener = App.Services.GetRequiredService<IDocumentOpener>();

    public MainViewModel ViewModel { get; } = App.Services.GetRequiredService<MainViewModel>();

    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppWindow.SetIcon("Assets/AppIcon.ico");
        AppWindow.Resize(new Windows.Graphics.SizeInt32(1280, 820));

        App.Services.GetRequiredService<INavigationService>().Attach(ContentFrame);
        ContentFrame.Navigated += OnFrameNavigated;
        ViewModel.Navigate("home");
    }

    private void OnPaneToggleRequested(TitleBar sender, object args) =>
        NavView.IsPaneOpen = !NavView.IsPaneOpen;

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
            e.DragUIOverride.Caption = "Open PDF";
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
