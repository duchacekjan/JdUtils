using JdUtils.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace JdUtils.Converters
{
    public class EnumToListConverter : AValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IEnumerable<KeyValuePair<object, string>> result = new Dictionary<object, string>();
            if (value is Type type && type.IsEnum)
            {
                result = Enum.GetValues(type)
                    .Cast<object>()
                    .Where(w => w != null)
                    .Select(s => GetDescription(s, type));
            }
            return result;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        private KeyValuePair<object, string> GetDescription(object enumValue, Type enumType)
        {
            var value = enumValue.ToString();
            var attribute = value.GetEnumAttribute<DescriptionAttribute>(enumType);

            var description = attribute?.Description ?? value;
            return new KeyValuePair<object, string>(enumValue, description);
        }
    }
}
