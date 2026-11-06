using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PaperFlow.Models;
using PaperFlow.ViewModels;

namespace PaperFlow.Views;

public sealed partial class HomePage : Page
{
    public LibraryViewModel ViewModel { get; } = App.Services.GetRequiredService<LibraryViewModel>();

    public HomePage()
    {
        InitializeComponent();
        Recent.ItemsSource = ViewModel.Documents;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e) =>
        ViewModel.Activate(LibraryFilter.Recent, limit: 8);

    protected override void OnNavigatedFrom(NavigationEventArgs e) => ViewModel.Deactivate();

    private async void OnOpenRequested(object? sender, PdfDocumentInfo doc) => await ViewModel.OpenAsync(doc);
    private async void OnFavoriteToggled(object? sender, PdfDocumentInfo doc) => await ViewModel.ToggleFavoriteAsync(doc);
    private async void OnRemoveRequested(object? sender, PdfDocumentInfo doc) => await ViewModel.RemoveAsync(doc);
}
