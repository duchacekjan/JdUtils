using System;
using System.Globalization;
using System.Windows;

namespace JdUtils.Converters
{
    public class RatioConverter : AValueConverter
    {
        public double Ratio { get; set; } = 1;

        public double MinValue { get; set; } = double.NegativeInfinity;

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = DependencyProperty.UnsetValue;
            if (value is double number)
            {
                var newNumber = number * Ratio;
                if (newNumber < MinValue)
                {
                    newNumber = MinValue;
                }
                result = newNumber;
            }
            return result;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
