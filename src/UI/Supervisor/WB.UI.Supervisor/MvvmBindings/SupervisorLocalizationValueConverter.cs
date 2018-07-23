using System;
using System.Globalization;
using WB.Core.BoundedContexts.Supervisor.Properties;
using WB.UI.Shared.Enumerator.Converters;

namespace WB.UI.Supervisor.MvvmBindings
{
    public class SupervisorLocalizationValueConverter : EnumeratorLocalizationValueConverter
    {
        protected override string Convert(string value, Type targetType, object parameter, CultureInfo culture)
        {
            var translation = base.Convert(value, targetType, parameter, culture);

            return string.IsNullOrEmpty(translation)
                ? SupervisorUIResources.ResourceManager.GetString(value, culture)
                : translation;
        }
    }
}
