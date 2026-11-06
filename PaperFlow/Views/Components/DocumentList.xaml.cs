using System.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PaperFlow.Models;

namespace PaperFlow.Views.Components;

public sealed partial class DocumentList : UserControl
{
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(DocumentList),
            new PropertyMetadata(null, (d, e) => ((DocumentList)d).List.ItemsSource = e.NewValue));

    public DocumentList()
    {
        InitializeComponent();
    }

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public event EventHandler<PdfDocumentInfo>? OpenRequested;
    public event EventHandler<PdfDocumentInfo>? FavoriteToggled;
    public event EventHandler<PdfDocumentInfo>? RemoveRequested;

    private void OnItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is PdfDocumentInfo doc) OpenRequested?.Invoke(this, doc);
    }

    private void OnFavoriteClick(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).Tag is PdfDocumentInfo doc) FavoriteToggled?.Invoke(this, doc);
    }

    private void OnRemoveClick(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).Tag is PdfDocumentInfo doc) RemoveRequested?.Invoke(this, doc);
    }
}
