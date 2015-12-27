using System;
using System.Globalization;
using Cirrious.CrossCore.Converters;
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

            if (string.Compare(culture.TwoLetterISOLanguageName, "in", StringComparison.InvariantCultureIgnoreCase) != 0)
                culture = indonesianCultureInfo;

            return UIResources.ResourceManager.GetString(value, culture);
        }
    }
}