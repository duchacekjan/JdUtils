using System;
using System.Globalization;
using System.Windows;

namespace JdUtils.Converters
{
    public class StringNullOrEmptyConverter : AValueWithNegateConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = string.IsNullOrEmpty(value?.ToString());
            if (Negate)
            {
                result = !result;
            }

            return result;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
