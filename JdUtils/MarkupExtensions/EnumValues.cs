using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Markup;
using JdUtils.Extensions;

namespace JdUtils.MarkupExtensions
{
    [MarkupExtensionReturnType(typeof(IEnumerable<KeyValuePair<object, string>>))]
    public class EnumValues : MarkupExtension
    {
        public Type EnumType { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            IEnumerable<KeyValuePair<object, string>> result = null;
            if (EnumType?.IsEnum == true)
            {
                result = Enum.GetValues(EnumType)
                    .Cast<object>()
                    .Where(w => w != null)
                    .Select(s => GetDescription(s, EnumType));
            }
            return result;
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
