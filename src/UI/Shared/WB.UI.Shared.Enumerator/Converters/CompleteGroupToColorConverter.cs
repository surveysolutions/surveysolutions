using System.Globalization;
using MvvmCross.Converters;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.UI.Shared.Enumerator.Converters;

public class CompleteGroupToColorConverter : MvxValueConverter<CompleteGroup, int>
{
    protected override int Convert(CompleteGroup completeGroup, Type targetType, object parameter, CultureInfo culture)
    {
        if (completeGroup.IsError)
            return Resource.Color.complete_screen_statistics_error_color;
        if (completeGroup.IsAnswered)
            return Resource.Color.complete_screen_statistics_answered_color;
        if (completeGroup.IsUnanswered)
            return Resource.Color.complete_screen_statistics_unanswered_color; 
        return Resource.Color.complete_screen_statistics_neutral_color;
    }
}
