using System;
using System.Globalization;
using Cirrious.CrossCore.Converters;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Properties;

namespace WB.UI.Interviewer.Converters
{
    public class InterviewerLocalizationValueConverter : MvxValueConverter<string, string>
    {
        protected override string Convert(string value, Type targetType, object parameter, CultureInfo culture)
        {
            var localizeString = InterviewerUIResources.ResourceManager.GetString(value);
            if (!localizeString.IsNullOrEmpty())
                return localizeString;
            return UIResources.ResourceManager.GetString(value);
        }
    }
}