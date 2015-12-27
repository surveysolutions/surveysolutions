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
        private static CultureInfo indonesianCultureInfo = CultureInfo.CreateSpecificCulture("id");

        protected override string Convert(string value, Type targetType, object parameter, CultureInfo culture)
        {
            if (culture == null)
                culture = CultureInfo.CurrentUICulture;

            if (string.Compare(culture.TwoLetterISOLanguageName, "in", StringComparison.InvariantCultureIgnoreCase) != 0)
                culture = indonesianCultureInfo;

            var localizeString = InterviewerUIResources.ResourceManager.GetString(value, culture);
            if (!localizeString.IsNullOrEmpty())
                return localizeString;

            return UIResources.ResourceManager.GetString(value, culture);
        }
    }
}