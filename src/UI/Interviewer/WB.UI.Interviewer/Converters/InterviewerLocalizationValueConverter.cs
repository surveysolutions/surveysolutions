using System;
using System.Globalization;
using MvvmCross.Platform.Converters;
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
            
            //hack for Indonesian culture android issue 
            if (culture.TwoLetterISOLanguageName.ToLower() == "iv")
            {
                if(Java.Util.Locale.Default.ISO3Country.ToUpper() == "IDN")
                    culture = indonesianCultureInfo;
            }

            try
            {
                var localizeString = InterviewerUIResources.ResourceManager.GetString(value, culture);
                if (!localizeString.IsNullOrEmpty())
                    return localizeString;
            
            return UIResources.ResourceManager.GetString(value, culture);
            }
            catch { }
            return value;
        }
    }
}