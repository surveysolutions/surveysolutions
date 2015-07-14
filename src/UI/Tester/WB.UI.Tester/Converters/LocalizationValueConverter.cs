using System;
using System.Globalization;
using Cirrious.CrossCore.Converters;
using WB.Core.BoundedContexts.Tester.Properties;

namespace WB.UI.Tester.Converters
{
    public class LocalizationValueConverter : MvxValueConverter<string, string>
    {
        protected override string Convert(string value, Type targetType, object parameter, CultureInfo culture)
        {
            return UIResources.ResourceManager.GetString(value);
        }
    }
}