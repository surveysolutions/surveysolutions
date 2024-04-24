using System.Globalization;
using MvvmCross.Converters;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.UI.Shared.Enumerator.Converters;

public class CompleteItemToColorConverter : MvxValueConverter<EntityWithErrorsViewModel, int>
{
    protected override int Convert(EntityWithErrorsViewModel item, Type targetType, object parameter, CultureInfo culture)
    {
        if (item.IsError)
            return Resource.Color.complete_screen_statistics_error_color; 
        return Resource.Color.complete_screen_statistics_neutral_color;
    }
}
