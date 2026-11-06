using Microsoft.UI.Xaml.Controls;
using PdfReader.Models;
using PdfReader.Views;

namespace PdfReader.Services;

public interface INavigationService
{
    void Attach(Frame frame);
    void NavigateTo(string tag);
    void OpenDocument(string path);
    void GoBack();
}

public sealed class NavigationService : INavigationService
{
    private Frame? _frame;

    public void Attach(Frame frame) => _frame = frame;

    public void OpenDocument(string path) => _frame?.Navigate(typeof(ViewerPage), path);

    public void GoBack()
    {
        if (_frame is { CanGoBack: true }) _frame.GoBack();
        else NavigateTo("home");
    }

    public void NavigateTo(string tag)
    {
        if (_frame is null) return;

        var (page, parameter) = tag switch
        {
            "recent" => (typeof(LibraryPage), (object?)LibraryFilter.Recent),
            "all" => (typeof(LibraryPage), LibraryFilter.All),
            "favorites" => (typeof(LibraryPage), LibraryFilter.Favorites),
            "bookmarks" => (typeof(LibraryPage), LibraryFilter.Bookmarks),
            "settings" => (typeof(SettingsPage), null),
            _ => (typeof(HomePage), null),
        };
        _frame.Navigate(page, parameter);
    }
}
