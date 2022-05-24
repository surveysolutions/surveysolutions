using System;
using System.Globalization;
using WB.Core.BoundedContexts.Tester.Properties;
using WB.UI.Shared.Enumerator.Converters;

namespace WB.UI.Tester.Converters
{
    public class TesterLocalizationValueConverter : EnumeratorLocalizationValueConverter
    {
        protected override string Convert(string value, Type targetType, object parameter, CultureInfo culture)
        {
            var translation = base.Convert(value, targetType, parameter, culture);

            return string.IsNullOrEmpty(translation)
                ? TesterUIResources.ResourceManager.GetString(value, culture)
                : translation;
        }
    }
}
