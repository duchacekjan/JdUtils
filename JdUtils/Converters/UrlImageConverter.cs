using JdUtils.Extensions;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace JdUtils.Converters
{
    public class UrlImageConverter : AValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = DependencyProperty.UnsetValue;
            if (value is string urlImage && targetType==typeof(ImageSource))
            {
                result = urlImage.ToImageSource();
            }

            return result;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
