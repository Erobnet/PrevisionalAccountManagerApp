using System.Globalization;
using System.Windows.Data;
using PrevisionalAccountManager.ViewModels;

namespace PrevisionalAccountManager.Converters
{
    public class ColorThemeConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if ( value is ColorTheme colorTheme )
            {
                return colorTheme.ToStringFast();
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if ( value is string str && ColorThemeExtensions.TryParse(str, out var colorTheme) )
            {
                return colorTheme;
            }
            return value;
        }
    }
}