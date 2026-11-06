using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PdfReader.Models;
using PdfReader.ViewModels;

namespace PdfReader.Views;

public sealed partial class LibraryPage : Page
{
    public LibraryViewModel ViewModel { get; } = App.Services.GetRequiredService<LibraryViewModel>();

    public LibraryPage()
    {
        InitializeComponent();
        Documents.ItemsSource = ViewModel.Documents;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e) =>
        ViewModel.Activate(e.Parameter is LibraryFilter filter ? filter : LibraryFilter.Recent);

    protected override void OnNavigatedFrom(NavigationEventArgs e) => ViewModel.Deactivate();

    private async void OnOpenRequested(object? sender, PdfDocumentInfo doc) => await ViewModel.OpenAsync(doc);
    private async void OnFavoriteToggled(object? sender, PdfDocumentInfo doc) => await ViewModel.ToggleFavoriteAsync(doc);
    private async void OnRemoveRequested(object? sender, PdfDocumentInfo doc) => await ViewModel.RemoveAsync(doc);
}
