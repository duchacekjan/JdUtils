﻿using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace JdComponents.Utils.Converters
{
    /// <summary>
    /// Common base for value converters with support of <see cref="MarkupExtension"/>
    /// </summary>
    [MarkupExtensionReturnType(typeof(IValueConverter))]
    public abstract class AValueConverter : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);

        public abstract object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);
    }
}