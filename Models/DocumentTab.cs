using CommunityToolkit.Mvvm.ComponentModel;
using PdfReader.Core;

namespace PdfReader.Models;

/// <summary>An open document in the tab bar. Its viewer, if it has one, is owned by the tab service.</summary>
public sealed partial class DocumentTab : ObservableObject
{
    public DocumentTab(string path, ViewState? view)
    {
        Path = path;
        Title = System.IO.Path.GetFileName(path);
        SavedView = view;
    }

    public string Path { get; }

    public string Title { get; }

    /// <summary>Reading position kept while the tab has no live viewer (not opened yet, or unloaded to save memory).</summary>
    public ViewState? SavedView { get; set; }

    /// <summary>When the tab was last shown; the least recently used viewers are unloaded first.</summary>
    public long LastActive { get; set; }

    [ObservableProperty]
    public partial bool IsActive { get; set; }
}
