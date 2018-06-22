using System;
using System.Globalization;
using MvvmCross.Converters;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Properties;

namespace WB.UI.Shared.Enumerator.Converters
{
    public class EnumeratorLocalizationValueConverter : MvxValueConverter<string, string>
    {
        private static readonly CultureInfo IndonesianCultureInfo = CultureInfo.CreateSpecificCulture("id");

        protected override string Convert(string value, Type targetType, object parameter, CultureInfo culture)
        {
            if (culture == null)
                culture = CultureInfo.CurrentUICulture;
            
            //hack for Indonesian culture android issue 
            if (culture.TwoLetterISOLanguageName.ToLower() == "iv")
            {
                if(Java.Util.Locale.Default.ISO3Country.ToUpper() == "IDN")
                    culture = IndonesianCultureInfo;
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
