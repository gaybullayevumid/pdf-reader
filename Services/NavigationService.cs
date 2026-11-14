using Microsoft.UI.Xaml.Controls;
using PdfReader.Models;
using PdfReader.Views;

namespace PdfReader.Services;

/// <summary>
/// Sidebar navigation between the library and settings pages. Documents live in tabs
/// (<see cref="IDocumentTabService"/>): showing a page only hides the active document, it never closes a tab.
/// </summary>
public interface INavigationService
{
    void Attach(Frame frame);
    void NavigateTo(string tag);
    void OpenDocument(string path);

    /// <summary>Leaves the document being read for the page underneath it (the tab stays open).</summary>
    void GoBack();
}

public sealed class NavigationService(IDocumentTabService tabs) : INavigationService
{
    private Frame? _frame;

    public void Attach(Frame frame) => _frame = frame;

    public void OpenDocument(string path) => tabs.Open(path);

    public void GoBack()
    {
        if (tabs.ActiveTab is not null) tabs.Deactivate();
        else if (_frame is { CanGoBack: true }) _frame.GoBack();
        else NavigateTo("home");
    }

    public void NavigateTo(string tag)
    {
        tabs.Deactivate();
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
