using System;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace JdUtils.MarkupExtensions
{
    [MarkupExtensionReturnType(typeof(ImageSource))]
    public class UrlImage : MarkupExtension
    {
        public string ImageLink { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            ImageSource result = null;
            if (!string.IsNullOrEmpty(ImageLink))
            {
                var bitmap = new BitmapImage();
                try
                {
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(ImageLink, UriKind.Absolute);
                    bitmap.EndInit();
                    result = bitmap;
                }
                catch {}
            }

            return result;
        }
    }
}
