using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PdfReader.Localization;

namespace PdfReader.Views.Components;

/// <summary>
/// One row of the Settings page: icon, title, optional description and a control on the right.
/// Titles come from a localization key (or plain text for dynamic content). When the card gets narrow the
/// control moves under the text so long translations never get clipped.
/// </summary>
public sealed partial class SettingCard : UserControl
{
    private const double WrapBelowWidth = 560;

    public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register(
        nameof(Glyph), typeof(string), typeof(SettingCard), new PropertyMetadata(string.Empty, (d, e) => ((SettingCard)d).Icon.Glyph = (string)e.NewValue));

    public static readonly DependencyProperty TitleKeyProperty = DependencyProperty.Register(
        nameof(TitleKey), typeof(string), typeof(SettingCard), new PropertyMetadata(null, (d, e) => Loc.SetText(((SettingCard)d).TitleText, e.NewValue as string)));

    public static readonly DependencyProperty DescriptionKeyProperty = DependencyProperty.Register(
        nameof(DescriptionKey), typeof(string), typeof(SettingCard), new PropertyMetadata(null, (d, e) =>
        {
            var card = (SettingCard)d;
            card.DescriptionText.Visibility = e.NewValue is null ? Visibility.Collapsed : Visibility.Visible;
            Loc.SetText(card.DescriptionText, e.NewValue as string);
        }));

    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(SettingCard), new PropertyMetadata(null, (d, e) => ((SettingCard)d).TitleText.Text = e.NewValue as string ?? string.Empty));

    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        nameof(Description), typeof(string), typeof(SettingCard), new PropertyMetadata(null, (d, e) =>
        {
            var card = (SettingCard)d;
            card.DescriptionText.Text = e.NewValue as string ?? string.Empty;
            card.DescriptionText.Visibility = string.IsNullOrEmpty(card.DescriptionText.Text) ? Visibility.Collapsed : Visibility.Visible;
        }));

    public static readonly DependencyProperty ActionContentProperty = DependencyProperty.Register(
        nameof(ActionContent), typeof(object), typeof(SettingCard), new PropertyMetadata(null, (d, e) => ((SettingCard)d).ActionPresenter.Content = e.NewValue));

    public SettingCard()
    {
        InitializeComponent();
    }

    public string Glyph { get => (string)GetValue(GlyphProperty); set => SetValue(GlyphProperty, value); }
    public string? TitleKey { get => (string?)GetValue(TitleKeyProperty); set => SetValue(TitleKeyProperty, value); }
    public string? DescriptionKey { get => (string?)GetValue(DescriptionKeyProperty); set => SetValue(DescriptionKeyProperty, value); }
    public string? Title { get => (string?)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public string? Description { get => (string?)GetValue(DescriptionProperty); set => SetValue(DescriptionProperty, value); }
    public object? ActionContent { get => GetValue(ActionContentProperty); set => SetValue(ActionContentProperty, value); }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var narrow = e.NewSize.Width < WrapBelowWidth;
        Grid.SetRow(ActionPresenter, narrow ? 1 : 0);
        Grid.SetColumn(ActionPresenter, narrow ? 1 : 2);
        Grid.SetColumnSpan(ActionPresenter, narrow ? 2 : 1);
        ActionPresenter.HorizontalAlignment = narrow ? HorizontalAlignment.Left : HorizontalAlignment.Right;
    }
}
