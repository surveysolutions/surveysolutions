using System;
using System.Globalization;
using WB.Core.SharedKernels.Enumerator.Properties;

namespace WB.UI.Shared.Enumerator.Converters
{
    public class EnumeratorLocalizationValueConverter : LocalizationValueConverter
    {
        protected override string Convert(string value, Type targetType, object parameter, CultureInfo culture)
        {
            var translation = base.Convert(value, targetType, parameter, culture);

            return string.IsNullOrEmpty(translation)
                ? InterviewerUIResources.ResourceManager.GetString(value, culture)
                : translation;
        }
    }
}
