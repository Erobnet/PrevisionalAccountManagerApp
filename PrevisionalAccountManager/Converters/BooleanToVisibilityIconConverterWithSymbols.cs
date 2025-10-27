using System;
using System.Globalization;
using System.Windows.Data;

namespace PrevisionalAccountManager.Converters
{
    public class BooleanToVisibilityIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isVisible)
            {
                return isVisible ? "\uE7B3" : "\uED1A"; // Eye open / Eye closed icons
            }
            return "\uED1A"; // Default to eye closed
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
