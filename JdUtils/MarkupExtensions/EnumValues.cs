using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Resources;
using System.Windows.Markup;
using JdUtils.Extensions;

namespace JdUtils.MarkupExtensions
{
    [MarkupExtensionReturnType(typeof(IEnumerable<KeyValuePair<object, string>>))]
    public class EnumValues : MarkupExtension
    {
        public EnumValues()
            : this(null)
        {
        }

        public EnumValues(Type enumType)
        {
            EnumType = enumType;
        }

        public ResourceManager Resources { get; set; }

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
            string description;
            if (Resources != null)
            {
                description = Resources.GetString($"{EnumType.Name}_{value}");
            }
            else
            {
                var attribute = value.GetEnumAttribute<DescriptionAttribute>(enumType);

                description = attribute?.Description;
            }

            return new KeyValuePair<object, string>(enumValue, description ?? value);
        }
    }
}
