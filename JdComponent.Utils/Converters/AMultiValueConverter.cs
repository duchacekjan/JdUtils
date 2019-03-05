using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace JdComponent.Utils.Converters
{
    /// <summary>
    /// Common base for multi value converters with support of <see cref="MarkupExtension"/>
    /// </summary>
    [MarkupExtensionReturnType(typeof(IMultiValueConverter))]
    public abstract class AMultiValueConverter : MarkupExtension, IMultiValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public abstract object Convert(object[] values, Type targetType, object parameter, CultureInfo culture);

        public abstract object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture);
    }
}
