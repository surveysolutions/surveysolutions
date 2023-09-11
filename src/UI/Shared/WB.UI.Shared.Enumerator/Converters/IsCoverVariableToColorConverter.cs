using System;
using System.Globalization;
using MvvmCross.Converters;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;

namespace WB.UI.Shared.Enumerator.Converters
{
    public class IsCoverVariableToColorConverter : MvxValueConverter<bool, int>
    {
        protected override int Convert(bool isCover, Type targetType, object parameter, CultureInfo culture)
        {
            return isCover 
                ? Resource.Color.variable_background_cover 
                : Resource.Color.variable_background;
        }
    }
}
