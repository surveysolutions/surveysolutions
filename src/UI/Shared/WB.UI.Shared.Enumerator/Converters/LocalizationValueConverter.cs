using System;
using System.Globalization;
using Cirrious.CrossCore.Converters;
using WB.Core.SharedKernels.Enumerator.Properties;

namespace WB.UI.Shared.Enumerator.Converters
{
    public class LocalizationValueConverter : MvxValueConverter<string, string>
    {
        protected override string Convert(string value, Type targetType, object parameter, CultureInfo culture)
        {
            return UIResources.ResourceManager.GetString(value);
        }
    }
}