﻿using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace JdComponent.Utils.Converters
{
    /// <summary>
    /// Converter for debugging purposes.
    /// </summary>
    public class DebugValueConverter : MarkupExtension, IValueConverter, IMultiValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new[] { value };
        }
    }
}
