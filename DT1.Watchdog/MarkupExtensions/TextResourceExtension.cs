using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DT1.Watchdog
{
    [ContentProperty("Text")]
    public class TextResourceExtension : IMarkupExtension
    {
        public string ResourceKey { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (ResourceKey == null)
            {
                return null;
            }

            return EmbeddedResource.ResourceManager.GetString(ResourceKey);
        }
    }
}
