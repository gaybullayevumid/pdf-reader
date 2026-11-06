using Microsoft.UI.Xaml.Data;

namespace PdfReader.Converters;

/// <summary>Filled star (E735) for favorites, outline star (E734) otherwise.</summary>
public sealed class FavoriteGlyphConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value is true ? "" : "";

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}
