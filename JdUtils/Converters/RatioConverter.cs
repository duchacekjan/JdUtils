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
            switch (value)
            {
                case double number:
                    result = ConvertDouble(number);
                    break;
                case Thickness thickness:
                    result = ConvertThickness(thickness);
                    break;
            }

            return result;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        private double ConvertDouble(double number)
        {
            var newNumber = number * Ratio;
            if (newNumber < MinValue)
            {
                newNumber = MinValue;
            }
            return newNumber;
        }

        private Thickness ConvertThickness(Thickness thickness)
        {
            var newLeft = ConvertDouble(thickness.Left);
            thickness.Left = newLeft;
            var newTop = ConvertDouble(thickness.Top);
            thickness.Top = newTop;
            var newRight = ConvertDouble(thickness.Right);
            thickness.Right = newRight;
            var newBottom = ConvertDouble(thickness.Bottom);
            thickness.Bottom = newBottom;
            return thickness;
        }
    }
}
