using System;
using System.Globalization;
using MvvmCross.Platform.Converters;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;

namespace WB.UI.Interviewer.Converters
{
    public class InterviewStatusToButtonConverter : MvxValueConverter<DashboardInterviewStatus, int>
    {
        protected override int Convert(DashboardInterviewStatus status, Type targetType, object parameter, CultureInfo culture)
        {
            switch (status)
            {
                case DashboardInterviewStatus.Assignment:
                case DashboardInterviewStatus.New:
                    return Resource.Drawable.default_input_button;

                case DashboardInterviewStatus.InProgress:
                    return Resource.Drawable.default_input_button;

                case DashboardInterviewStatus.Completed:
                    return Resource.Drawable.group_completed;

                case DashboardInterviewStatus.Rejected:
                    return Resource.Drawable.group_with_invalid_answers;
            }

            return Resource.Drawable.default_input_button;
        }
    }
}