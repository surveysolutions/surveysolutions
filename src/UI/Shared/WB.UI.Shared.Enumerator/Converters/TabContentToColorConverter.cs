using System.Globalization;
using MvvmCross.Converters;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.UI.Shared.Enumerator.Converters;

public class TabContentToColorConverter : MvxValueConverter<TabViewModel, int>
{
    protected override int Convert(TabViewModel tabViewModel, Type targetType, object parameter, CultureInfo culture)
    {
        return GetColor(tabViewModel);
    }

    public static int GetColor(TabViewModel tabViewModel)
    {
        if (tabViewModel == null || !tabViewModel.IsEnabled)
            return Resource.Color.disabledTextColor;
        if (tabViewModel.TabContent == CompleteTabContent.CriticalError)
            return Resource.Color.complete_screen_statistics_error_color;
        if (tabViewModel.TabContent == CompleteTabContent.Error)
            return Resource.Color.complete_screen_statistics_error_color;
        // if (tabViewModel.GroupContent == CompleteGroupContent.Answered)
        //     return Resource.Color.complete_screen_statistics_answered_color;
        if (tabViewModel.TabContent == CompleteTabContent.Unanswered)
            return Resource.Color.complete_screen_statistics_unanswered_color; 
        return Resource.Color.complete_screen_statistics_neutral_color;
    }
}
