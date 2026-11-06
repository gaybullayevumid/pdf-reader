using Microsoft.Extensions.Logging;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using PdfReader.Core;
using Windows.Graphics;

namespace PdfReader.Services;

public interface IWindowStateService
{
    bool IsFullScreen { get; }
    event EventHandler? FullScreenChanged;

    void Attach(Window window);

    /// <summary>Applies the saved size and position. Call before the window is shown.</summary>
    void RestoreBounds();

    /// <summary>Applies the saved maximized / full screen mode. Call right after the window is activated.</summary>
    void RestoreMode();

    void SetFullScreen(bool fullScreen);
    void ToggleFullScreen();

    /// <summary>Back to the default size, centered, in normal mode.</summary>
    void ResetLayout();

    /// <summary>Writes the current state immediately (used on close).</summary>
    void Flush();
}

/// <summary>
/// Keeps the main window's mode (normal / maximized / full screen) and its restored bounds in the settings file.
/// Changes are saved a moment after they happen, so even a crash loses at most the last few hundred milliseconds.
/// </summary>
public sealed class WindowStateService : IWindowStateService
{
    private readonly ISettingsService _settings;
    private readonly ILogger<WindowStateService> _logger;

    private Window? _window;
    private AppWindow? _appWindow;
    private DispatcherQueueTimer? _saveTimer;

    private RectI _normal = new(0, 0, WindowPlacement.DefaultWidth, WindowPlacement.DefaultHeight);
    private bool _hasNormal;
    private bool _wasMaximized;
    private bool _applying;
    private bool _fullScreenReported;

    public WindowStateService(ISettingsService settings, ILogger<WindowStateService> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public event EventHandler? FullScreenChanged;

    public bool IsFullScreen => _appWindow?.Presenter.Kind == AppWindowPresenterKind.FullScreen;

    public void Attach(Window window)
    {
        _window = window;
        _appWindow = window.AppWindow;

        _saveTimer = window.DispatcherQueue.CreateTimer();
        _saveTimer.Interval = TimeSpan.FromMilliseconds(400);
        _saveTimer.IsRepeating = false;
        _saveTimer.Tick += (_, _) => Flush();

        if (_appWindow.Presenter is OverlappedPresenter overlapped)
        {
            // Below this the sidebar and toolbars no longer fit; keeps the layout usable.
            overlapped.PreferredMinimumWidth = WindowPlacement.MinWidth;
            overlapped.PreferredMinimumHeight = WindowPlacement.MinHeight;
        }

        _appWindow.Changed += OnAppWindowChanged;
        window.Closed += (_, _) => Flush();
    }

    public void RestoreBounds()
    {
        if (_appWindow is null) return;

        var saved = _settings.RememberWindowState ? _settings.Window : null;
        var state = WindowPlacement.Sanitize(saved, WorkAreas());
        _wasMaximized = state.WasMaximized || state.Mode == WindowMode.Maximized;

        var bounds = new RectI(state.X ?? 0, state.Y ?? 0, state.Width, state.Height);
        ApplyBounds(bounds, state.X.HasValue && state.Y.HasValue);
        _normal = bounds;
        _hasNormal = true;
    }

    public void RestoreMode()
    {
        if (_appWindow is null || !_settings.RememberWindowState) return;

        var mode = WindowPlacement.Sanitize(_settings.Window, WorkAreas()).Mode;
        _applying = true;
        try
        {
            switch (mode)
            {
                case WindowMode.FullScreen:
                    _appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
                    break;
                case WindowMode.Maximized:
                    (_appWindow.Presenter as OverlappedPresenter)?.Maximize();
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not restore window mode {Mode}", mode);
        }
        finally
        {
            _applying = false;
        }
        NotifyFullScreen();
    }

    public void ToggleFullScreen() => SetFullScreen(!IsFullScreen);

    public void SetFullScreen(bool fullScreen)
    {
        if (_appWindow is null || fullScreen == IsFullScreen) return;

        _applying = true;
        try
        {
            if (fullScreen)
            {
                if (_appWindow.Presenter is OverlappedPresenter { State: OverlappedPresenterState.Maximized })
                    _wasMaximized = true;
                _appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
            }
            else
            {
                _appWindow.SetPresenter(AppWindowPresenterKind.Overlapped);
                if (_hasNormal) ApplyBounds(_normal, hasPosition: true);
                if (_wasMaximized) (_appWindow.Presenter as OverlappedPresenter)?.Maximize();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not change full screen mode");
        }
        finally
        {
            _applying = false;
        }

        NotifyFullScreen();
        ScheduleSave();
    }

    public void ResetLayout()
    {
        if (_appWindow is null) return;

        _applying = true;
        try
        {
            _appWindow.SetPresenter(AppWindowPresenterKind.Overlapped);
            (_appWindow.Presenter as OverlappedPresenter)?.Restore();
            _wasMaximized = false;

            var state = WindowPlacement.Sanitize(new WindowStateData(), WorkAreas());
            _normal = new RectI(state.X ?? 0, state.Y ?? 0, state.Width, state.Height);
            _hasNormal = true;
            ApplyBounds(_normal, state.X.HasValue && state.Y.HasValue);
        }
        finally
        {
            _applying = false;
        }

        NotifyFullScreen();
        Flush();
    }

    public void Flush()
    {
        _saveTimer?.Stop();
        if (_appWindow is null || !_settings.RememberWindowState) return;

        try
        {
            _settings.Window = Capture();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not save window state");
        }
    }

    private WindowStateData Capture()
    {
        var mode = _appWindow!.Presenter switch
        {
            { Kind: AppWindowPresenterKind.FullScreen } => WindowMode.FullScreen,
            OverlappedPresenter { State: OverlappedPresenterState.Maximized } => WindowMode.Maximized,
            _ => WindowMode.Normal,
        };

        // Bounds always describe the restored window; while maximized or full screen the live size is the screen's.
        if (mode == WindowMode.Normal && _appWindow.Presenter is OverlappedPresenter { State: OverlappedPresenterState.Restored })
            TrackNormalBounds();

        return new WindowStateData(mode, _normal.X, _normal.Y, _normal.Width, _normal.Height,
            _wasMaximized || mode == WindowMode.Maximized);
    }

    private void OnAppWindowChanged(AppWindow sender, AppWindowChangedEventArgs args)
    {
        if (_applying) return;

        if (args.DidPresenterChange || args.DidSizeChange || args.DidPositionChange)
        {
            if (sender.Presenter is OverlappedPresenter presenter)
            {
                switch (presenter.State)
                {
                    case OverlappedPresenterState.Restored:
                        _wasMaximized = false;
                        TrackNormalBounds();
                        break;
                    case OverlappedPresenterState.Maximized:
                        _wasMaximized = true;
                        break;
                }
            }
            ScheduleSave();
        }

        if (args.DidPresenterChange) NotifyFullScreen();
    }

    private void TrackNormalBounds()
    {
        if (_appWindow is null) return;
        var size = _appWindow.Size;
        var position = _appWindow.Position;
        if (size.Width < WindowPlacement.MinWidth / 2 || size.Height < WindowPlacement.MinHeight / 2) return; // minimizing
        _normal = new RectI(position.X, position.Y, size.Width, size.Height);
        _hasNormal = true;
    }

    private void ApplyBounds(RectI bounds, bool hasPosition)
    {
        if (_appWindow is null) return;
        var wasApplying = _applying;
        _applying = true;
        try
        {
            if (hasPosition)
                _appWindow.MoveAndResize(new RectInt32(bounds.X, bounds.Y, bounds.Width, bounds.Height));
            else
                _appWindow.Resize(new SizeInt32(bounds.Width, bounds.Height));
        }
        finally
        {
            _applying = wasApplying;
        }
    }

    private void ScheduleSave()
    {
        if (_saveTimer is null || !_settings.RememberWindowState) return;
        _saveTimer.Stop();
        _saveTimer.Start();
    }

    private void NotifyFullScreen()
    {
        var now = IsFullScreen;
        if (now == _fullScreenReported) return;
        _fullScreenReported = now;
        FullScreenChanged?.Invoke(this, EventArgs.Empty);
    }

    private IReadOnlyList<RectI> WorkAreas()
    {
        try
        {
            var areas = DisplayArea.FindAll();
            var list = new List<RectI>(areas.Count);
            var primary = DisplayArea.Primary.WorkArea;
            list.Add(new RectI(primary.X, primary.Y, primary.Width, primary.Height));
            for (var i = 0; i < areas.Count; i++)
            {
                var w = areas[i].WorkArea;
                var rect = new RectI(w.X, w.Y, w.Width, w.Height);
                if (!list.Contains(rect)) list.Add(rect);
            }
            return list;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not read display areas");
            return [];
        }
    }
}
