using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using PdfReader.Core;
using PdfReader.Database;
using PdfReader.Models;
using PdfReader.Views;

namespace PdfReader.Services;

/// <summary>
/// Document management, independent of sidebar navigation: the open tabs, which one is showing, and their viewers.
/// Library and settings pages never close tabs; they only hide the active document (<see cref="Deactivate"/>).
/// </summary>
public interface IDocumentTabService
{
    ObservableCollection<DocumentTab> Tabs { get; }

    /// <summary>The tab being shown, or null while a library or settings page is showing.</summary>
    DocumentTab? ActiveTab { get; }

    /// <summary>The viewer of <see cref="ActiveTab"/>.</summary>
    ViewerPage? ActiveViewer { get; }

    event EventHandler? ActiveChanged;

    /// <summary>
    /// Opens a document in a new tab next to the active one, or finds its existing tab, and shows it unless
    /// <paramref name="activate"/> is false (a background tab loads when it is first shown).
    /// </summary>
    void Open(string path, bool activate = true);

    void Activate(DocumentTab tab);

    /// <summary>Hides the active document (e.g. when the sidebar shows a page); every tab stays open.</summary>
    void Deactivate();

    /// <summary>Activates the tab <paramref name="step"/> places away, wrapping around.</summary>
    void Cycle(int step);

    void Close(DocumentTab tab);

    /// <summary>Reopens the tabs of the previous run; only the active one is loaded right away.</summary>
    void RestoreSession();

    /// <summary>Writes the session now (on exit); otherwise it is written shortly after every change.</summary>
    void SaveSession();
}

public sealed class DocumentTabService : IDocumentTabService
{
    // Viewers of hidden tabs keep their document open for instant switching, up to this many; older ones are
    // unloaded (their position is kept) and reopen when their tab is shown again.
    private const int MaxLiveViewers = 5;

    private static readonly string SessionPath = Path.Combine(AppPaths.DataDirectory, "session.json");

    private readonly ILogger<DocumentTabService> _logger;
    private readonly Dictionary<DocumentTab, ViewerPage> _viewers = [];
    private DispatcherQueueTimer? _saveTimer;
    private long _clock;
    private bool _restoring;

    public DocumentTabService(ILogger<DocumentTabService> logger)
    {
        _logger = logger;
        Tabs.CollectionChanged += (_, _) => ScheduleSave(); // includes reordering by drag
    }

    public ObservableCollection<DocumentTab> Tabs { get; } = [];

    public DocumentTab? ActiveTab { get; private set; }

    public ViewerPage? ActiveViewer => ActiveTab is { } tab ? _viewers.GetValueOrDefault(tab) : null;

    public event EventHandler? ActiveChanged;

    public void Open(string path, bool activate = true)
    {
        path = TabRules.Normalize(path);
        var tab = Tabs.FirstOrDefault(t => TabRules.SamePath(t.Path, path));
        if (tab is null)
        {
            tab = new DocumentTab(path, view: null);
            var index = ActiveTab is { } active ? Tabs.IndexOf(active) + 1 : Tabs.Count; // next to the current tab
            Tabs.Insert(index, tab);
        }
        if (activate) Activate(tab);
    }

    public void Activate(DocumentTab tab)
    {
        if (!Tabs.Contains(tab) || tab == ActiveTab) return;

        if (ActiveTab is { } previous)
        {
            previous.IsActive = false;
            if (_viewers.TryGetValue(previous, out var hidden)) hidden.Suspend();
        }

        tab.IsActive = true;
        tab.LastActive = ++_clock;
        ActiveTab = tab;

        if (_viewers.TryGetValue(tab, out var viewer))
        {
            viewer.Resume();
        }
        else
        {
            viewer = CreateViewer(tab);
            _viewers[tab] = viewer;
            TrimViewers();
        }

        ActiveChanged?.Invoke(this, EventArgs.Empty);
        ScheduleSave();
    }

    public void Deactivate()
    {
        if (ActiveTab is not { } tab) return;
        tab.IsActive = false;
        if (_viewers.TryGetValue(tab, out var viewer)) viewer.Suspend();
        ActiveTab = null;
        ActiveChanged?.Invoke(this, EventArgs.Empty);
        ScheduleSave();
    }

    public void Cycle(int step)
    {
        if (Tabs.Count == 0) return;
        var current = ActiveTab is { } tab ? Tabs.IndexOf(tab) : (step > 0 ? -1 : 0);
        Activate(Tabs[((current + step) % Tabs.Count + Tabs.Count) % Tabs.Count]);
    }

    public void Close(DocumentTab tab)
    {
        var index = Tabs.IndexOf(tab);
        if (index < 0) return;

        var wasActive = tab == ActiveTab;
        var next = TabRules.ActiveAfterClose(index, ActiveTab is { } a ? Tabs.IndexOf(a) : -1, Tabs.Count);

        if (_viewers.Remove(tab, out var viewer)) viewer.Close();
        tab.IsActive = false;
        if (wasActive) ActiveTab = null;
        Tabs.RemoveAt(index);

        if (wasActive)
        {
            if (next >= 0) Activate(Tabs[next]);
            else ActiveChanged?.Invoke(this, EventArgs.Empty); // back to the library page underneath
        }
        ScheduleSave();
    }

    public void RestoreSession()
    {
        var session = SessionStore.Load(SessionPath, ex => _logger.LogWarning(ex, "Could not read the tab session"));
        _restoring = true;
        try
        {
            DocumentTab? active = null;
            for (var i = 0; i < session.Tabs.Count; i++)
            {
                var entry = session.Tabs[i];
                // A file that was moved or deleted since the last run would only open as an error.
                if (!File.Exists(entry.Path) || Tabs.Any(t => TabRules.SamePath(t.Path, entry.Path))) continue;
                var tab = new DocumentTab(TabRules.Normalize(entry.Path), entry.View);
                Tabs.Add(tab);
                if (i == session.Active) active = tab;
            }
            if (active is not null) Activate(active);
        }
        finally
        {
            _restoring = false;
        }
        _logger.LogInformation("Restored {Count} tab(s)", Tabs.Count);
    }

    public void SaveSession()
    {
        _saveTimer?.Stop();
        var entries = Tabs.Select(t => new TabEntry(t.Path, CurrentView(t))).ToList();
        var session = new SessionData(entries, ActiveTab is { } tab ? Tabs.IndexOf(tab) : -1);
        try
        {
            SessionStore.Save(SessionPath, session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not save the tab session");
        }
    }

    private ViewState? CurrentView(DocumentTab tab) =>
        _viewers.TryGetValue(tab, out var viewer) ? viewer.CaptureState() ?? tab.SavedView : tab.SavedView;

    private ViewerPage CreateViewer(DocumentTab tab)
    {
        var viewer = new ViewerPage();
        viewer.StateChanged += (_, _) => ScheduleSave();
        viewer.CloseRequested += (_, _) => Close(tab);
        viewer.Open(tab.Path, tab.SavedView);
        return viewer;
    }

    /// <summary>Unloads the least recently shown viewers beyond the limit; their tabs keep the reading position.</summary>
    private void TrimViewers()
    {
        while (_viewers.Count > MaxLiveViewers)
        {
            var (tab, viewer) = _viewers.Where(p => p.Key != ActiveTab).MinBy(p => p.Key.LastActive);
            tab.SavedView = viewer.CaptureState() ?? tab.SavedView;
            viewer.Close();
            _viewers.Remove(tab);
        }
    }

    private void ScheduleSave()
    {
        if (_restoring) return;
        if (_saveTimer is null)
        {
            if (DispatcherQueue.GetForCurrentThread() is not { } queue) return;
            _saveTimer = queue.CreateTimer();
            _saveTimer.Interval = TimeSpan.FromMilliseconds(500);
            _saveTimer.IsRepeating = false;
            _saveTimer.Tick += (_, _) => SaveSession();
        }
        _saveTimer.Stop();
        _saveTimer.Start();
    }
}
