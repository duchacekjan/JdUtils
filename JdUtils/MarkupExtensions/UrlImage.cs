﻿using JdUtils.Extensions;
using System;
using System.Windows.Markup;
using System.Windows.Media;

namespace JdUtils.MarkupExtensions
{
    [MarkupExtensionReturnType(typeof(ImageSource))]
    public class UrlImage : MarkupExtension
    {
        public string ImageLink { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return ImageLink?.ToImageSource();
        }
    }
}
