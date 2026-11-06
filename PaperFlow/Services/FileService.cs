using Microsoft.UI.Xaml;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace PaperFlow.Services;

/// <summary>Exposes the main window to services that need an HWND (pickers, dialogs).</summary>
public interface IWindowProvider
{
    Window Window { get; }
    void Set(Window window);
}

public sealed class WindowProvider : IWindowProvider
{
    private Window? _window;
    public Window Window => _window ?? throw new InvalidOperationException("Main window not created yet.");
    public void Set(Window window) => _window = window;
}

public interface IFileService
{
    /// <summary>Shows the system file picker. Returns the selected PDF paths, or empty.</summary>
    Task<IReadOnlyList<string>> PickPdfFilesAsync();

    /// <summary>Filters to existing files with a .pdf extension.</summary>
    IReadOnlyList<string> FilterPdfPaths(IEnumerable<string> paths);
}

public sealed class FileService(IWindowProvider windowProvider) : IFileService
{
    public async Task<IReadOnlyList<string>> PickPdfFilesAsync()
    {
        var picker = new FileOpenPicker { SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
        picker.FileTypeFilter.Add(".pdf");
        InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(windowProvider.Window));

        var files = await picker.PickMultipleFilesAsync();
        return files.Select(f => f.Path).ToList();
    }

    public IReadOnlyList<string> FilterPdfPaths(IEnumerable<string> paths) =>
        paths.Where(p => string.Equals(Path.GetExtension(p), ".pdf", StringComparison.OrdinalIgnoreCase) && File.Exists(p))
             .ToList();
}
