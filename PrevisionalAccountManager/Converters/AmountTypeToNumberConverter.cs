using System.Globalization;
using System.Windows.Data;
using PrevisionalAccountManager.Models;

namespace PrevisionalAccountManager.Converters;

public class AmountTypeToNumberConverter : IValueConverter
{
    private const string _numberFormat = "F2";

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string strFormat = parameter as string ?? _numberFormat;
        return value switch {
            Amount amount => amount.Value.ToString(strFormat, CultureInfo.CurrentUICulture),
            decimal d => d.ToString(strFormat, CultureInfo.CurrentUICulture),
            double d => d.ToString(strFormat, CultureInfo.CurrentUICulture),
            float f => f.ToString(strFormat, CultureInfo.CurrentUICulture),
            _ => value?.ToString()
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if ( value is string str )
        {
            if ( str is ['-'] )
            {
                return new Amount { Value = -1 };
            }

            if ( Amount.TryParse(str, CultureInfo.CurrentUICulture, out Amount result) )
            {
                return result;
            }
        }
        
        return new Amount { Value = 0 };
    }
}