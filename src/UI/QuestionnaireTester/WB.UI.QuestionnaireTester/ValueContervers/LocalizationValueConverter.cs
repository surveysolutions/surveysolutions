using System;
using System.Globalization;
using Cirrious.CrossCore.Converters;
using WB.UI.QuestionnaireTester.Properties;

namespace WB.UI.QuestionnaireTester.ValueContervers
{
    public class LocalizationValueConverter : MvxValueConverter<string, string>
    {
        protected override string Convert(string value, Type targetType, object parameter, CultureInfo culture)
        {
            return UIResources.ResourceManager.GetString(value);
        }
    }
}