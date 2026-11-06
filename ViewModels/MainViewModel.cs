using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PdfReader.Services;

namespace PdfReader.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly IDocumentOpener _opener;
    private readonly INavigationService _navigation;

    public MainViewModel(IDocumentOpener opener, INavigationService navigation)
    {
        _opener = opener;
        _navigation = navigation;
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

    [RelayCommand]
    private Task OpenPdfAsync() => _opener.PickAndOpenAsync();

    public void Navigate(string tag) => _navigation.NavigateTo(tag);
}
