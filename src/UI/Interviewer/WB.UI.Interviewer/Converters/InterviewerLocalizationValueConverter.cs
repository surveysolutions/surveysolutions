using System;
using System.Globalization;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.UI.Shared.Enumerator.Converters;

namespace WB.UI.Interviewer.Converters
{
    public class InterviewerLocalizationValueConverter : EnumeratorLocalizationValueConverter
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
