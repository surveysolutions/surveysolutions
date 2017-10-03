using System;
using System.Globalization;
using MvvmCross.Platform.Converters;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.GenericSubdomains.Portable;


namespace WB.UI.Interviewer.Converters
{
    public class InterviewStatusToColorConverter : MvxValueConverter<DashboardInterviewStatus, int>
    {
        protected override int Convert(DashboardInterviewStatus status, Type targetType, object parameter, CultureInfo culture)
        {
            switch (status)
            {
                case DashboardInterviewStatus.Assignment:
                case DashboardInterviewStatus.New:
                    return Resource.Color.dashboard_interview_subtitle;

                case DashboardInterviewStatus.InProgress:
                    return Resource.Color.dashboard_in_progress_tab;

                case DashboardInterviewStatus.Completed:
                    return Resource.Color.dashboard_completed_tab;

                case DashboardInterviewStatus.Rejected:
                    return Resource.Color.dashboard_rejected_tab;
            }

            throw new ArgumentException("status is unknown - {0}".FormatString(status));
        }
    }
}