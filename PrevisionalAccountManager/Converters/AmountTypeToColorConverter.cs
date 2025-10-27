using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using PrevisionalAccountManager.Models;
using PrevisionalAccountManager.ViewModels;

namespace PrevisionalAccountManager.Converters;

public class AmountTypeToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch {
            Amount amount => amount.Value >= 0 ? Brushes.Green : Brushes.Red,
            decimal d => d >= 0 ? Brushes.Green : Brushes.Red,
            double d => d >= 0 ? Brushes.Green : Brushes.Red,
            int i => i >= 0 ? Brushes.Green : Brushes.Red,
            long l => l >= 0 ? Brushes.Green : Brushes.Red,
            float f => f >= 0 ? Brushes.Green : Brushes.Red,
            _ => Brushes.Black
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class CategoryReadonlyToDisplayStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is CategoryViewModel category ? category.ToString() : value?.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}