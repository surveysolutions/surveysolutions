using System;
using System.Globalization;
using MvvmCross.Converters;
using WB.Core.SharedKernels.Enumerator.Properties;

namespace WB.UI.Shared.Enumerator.Converters
{
    public class LocalizationValueConverter : MvxValueConverter<string, string>
    {
        private static CultureInfo indonesianCultureInfo = CultureInfo.CreateSpecificCulture("id");

        protected override string Convert(string value, Type targetType, object parameter, CultureInfo culture)
        {

            if (culture == null)
                culture = CultureInfo.CurrentUICulture;

            //hack for Indonesian culture android issue 
            if (culture.TwoLetterISOLanguageName.ToLower() == "iv")
            {
                if (Java.Util.Locale.Default.ISO3Country.ToUpper() == "IDN")
                    culture = indonesianCultureInfo;
            }
            
            return UIResources.ResourceManager.GetString(value, culture);
        }
    }
}