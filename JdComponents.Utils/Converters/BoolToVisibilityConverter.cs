using System;
using System.Globalization;
using System.Windows;

namespace JdComponents.Utils.Converters
{
    /// <summary>
    /// Booleand to visibility converter
    /// </summary>
    public class BoolToVisibilityConverter : AValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = Visibility.Hidden;
            if (value is bool boolean)
            {
                result = boolean ? Visibility.Visible : Visibility.Collapsed;
            }

            return result;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object result;
            var visibility = value as Visibility?;
            switch (visibility)
            {
                case Visibility.Visible:
                    result = true;
                    break;
                case Visibility.Collapsed:
                    result = false;
                    break;
                default:
                    result = null;
                    break;
            }

            return result;
        }
    }
}
