using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using PdfReader.Services;

namespace PdfReader.Localization;

/// <summary>
/// XAML localization: <c>&lt;TextBlock loc:Loc.Text="settings.title" /&gt;</c>. Each attached property writes the
/// translated string into the matching property of the element and refreshes it when the language changes,
/// so no restart or page reload is needed.
/// </summary>
public static class Loc
{
    private static ILocalizationService? _service;
    private static readonly List<Binding> Bindings = [];

    private sealed record Binding(WeakReference<DependencyObject> Target, Func<DependencyObject, DependencyProperty?> Resolve, string Key);

    public static ILocalizationService Service
    {
        get => _service ?? throw new InvalidOperationException("Localization is not initialized.");
        set
        {
            if (_service is not null) _service.LanguageChanged -= OnLanguageChanged;
            _service = value;
            _service.LanguageChanged += OnLanguageChanged;
        }
    }

    public static string Get(string key) => Service.Get(key);

    public static string Format(string key, params object?[] args) => Service.Format(key, args);

    // ---- attached properties ------------------------------------------------------------------

    public static readonly DependencyProperty TextProperty = Register("Text", d => d switch
    {
        MenuFlyoutItem => MenuFlyoutItem.TextProperty,
        _ => TextBlock.TextProperty,
    });

    public static readonly DependencyProperty ContentProperty = Register("Content", _ => ContentControl.ContentProperty);

    public static readonly DependencyProperty LabelProperty = Register("Label", _ => AppBarButton.LabelProperty);

    public static readonly DependencyProperty HeaderProperty = Register("Header", d => d switch
    {
        ToggleSwitch => ToggleSwitch.HeaderProperty,
        ComboBox => ComboBox.HeaderProperty,
        NumberBox => NumberBox.HeaderProperty,
        _ => null,
    });

    public static readonly DependencyProperty ToolTipProperty = Register("ToolTip", _ => ToolTipService.ToolTipProperty);

    public static readonly DependencyProperty TitleProperty = Register("Title", d => d switch
    {
        InfoBar => InfoBar.TitleProperty,
        _ => null,
    });

    public static readonly DependencyProperty PlaceholderProperty = Register("Placeholder", d => d switch
    {
        PasswordBox => PasswordBox.PlaceholderTextProperty,
        TextBox => TextBox.PlaceholderTextProperty,
        ComboBox => ComboBox.PlaceholderTextProperty,
        _ => null,
    });

    public static readonly DependencyProperty AutomationNameProperty = Register("AutomationName", _ => AutomationProperties.NameProperty);

    public static readonly DependencyProperty OnContentProperty = Register("OnContent", _ => ToggleSwitch.OnContentProperty);

    public static readonly DependencyProperty OffContentProperty = Register("OffContent", _ => ToggleSwitch.OffContentProperty);

    public static string? GetText(DependencyObject d) => (string?)d.GetValue(TextProperty);
    public static void SetText(DependencyObject d, string? value) => d.SetValue(TextProperty, value);
    public static string? GetContent(DependencyObject d) => (string?)d.GetValue(ContentProperty);
    public static void SetContent(DependencyObject d, string? value) => d.SetValue(ContentProperty, value);
    public static string? GetLabel(DependencyObject d) => (string?)d.GetValue(LabelProperty);
    public static void SetLabel(DependencyObject d, string? value) => d.SetValue(LabelProperty, value);
    public static string? GetHeader(DependencyObject d) => (string?)d.GetValue(HeaderProperty);
    public static void SetHeader(DependencyObject d, string? value) => d.SetValue(HeaderProperty, value);
    public static string? GetToolTip(DependencyObject d) => (string?)d.GetValue(ToolTipProperty);
    public static void SetToolTip(DependencyObject d, string? value) => d.SetValue(ToolTipProperty, value);
    public static string? GetTitle(DependencyObject d) => (string?)d.GetValue(TitleProperty);
    public static void SetTitle(DependencyObject d, string? value) => d.SetValue(TitleProperty, value);
    public static string? GetPlaceholder(DependencyObject d) => (string?)d.GetValue(PlaceholderProperty);
    public static void SetPlaceholder(DependencyObject d, string? value) => d.SetValue(PlaceholderProperty, value);
    public static string? GetAutomationName(DependencyObject d) => (string?)d.GetValue(AutomationNameProperty);
    public static void SetAutomationName(DependencyObject d, string? value) => d.SetValue(AutomationNameProperty, value);
    public static string? GetOnContent(DependencyObject d) => (string?)d.GetValue(OnContentProperty);
    public static void SetOnContent(DependencyObject d, string? value) => d.SetValue(OnContentProperty, value);
    public static string? GetOffContent(DependencyObject d) => (string?)d.GetValue(OffContentProperty);
    public static void SetOffContent(DependencyObject d, string? value) => d.SetValue(OffContentProperty, value);

    /// <summary>Binds a property from code (for elements created at runtime or not reachable from XAML).</summary>
    public static void BindText(DependencyObject target, string key) => SetText(target, key);

    // ---- plumbing -----------------------------------------------------------------------------

    private static DependencyProperty Register(string name, Func<DependencyObject, DependencyProperty?> resolve) =>
        DependencyProperty.RegisterAttached(name, typeof(string), typeof(Loc),
            new PropertyMetadata(null, (d, e) => OnKeyChanged(d, resolve, e.NewValue as string)));

    private static void OnKeyChanged(DependencyObject target, Func<DependencyObject, DependencyProperty?> resolve, string? key)
    {
        if (string.IsNullOrEmpty(key) || _service is null) return;

        lock (Bindings)
        {
            Bindings.RemoveAll(b => !b.Target.TryGetTarget(out var t) || (ReferenceEquals(t, target) && ReferenceEquals(b.Resolve, resolve)));
            Bindings.Add(new Binding(new WeakReference<DependencyObject>(target), resolve, key));
        }
        Apply(target, resolve, key);
    }

    private static void Apply(DependencyObject target, Func<DependencyObject, DependencyProperty?> resolve, string key)
    {
        if (resolve(target) is not { } property) return;
        target.SetValue(property, Service.Get(key));
    }

    private static void OnLanguageChanged(object? sender, EventArgs e)
    {
        Binding[] snapshot;
        lock (Bindings)
        {
            Bindings.RemoveAll(b => !b.Target.TryGetTarget(out _));
            snapshot = [.. Bindings];
        }

        foreach (var binding in snapshot)
            if (binding.Target.TryGetTarget(out var target))
                Apply(target, binding.Resolve, binding.Key);
    }
}
