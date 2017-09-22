using System;
using System.Globalization;
using Android.Views;
using MvvmCross.Platform.Converters;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.GenericSubdomains.Portable;

namespace WB.UI.Interviewer.CustomBindings
{
    public class StatusToDasboardBackgroundConverter : MvxValueConverter<DashboardInterviewStatus, int>
    {
        protected override int Convert(DashboardInterviewStatus status, Type targetType, object parameter, CultureInfo culture)
        {
            switch (status)
            {
                case DashboardInterviewStatus.Assignment:
                    return Resource.Drawable.dashboard_interview_status_new;

                case DashboardInterviewStatus.New:
                case DashboardInterviewStatus.InProgress:
                    return Resource.Drawable.dashboard_interview_status_inprogress;

                case DashboardInterviewStatus.Completed:
                    return Resource.Drawable.dashboard_interview_status_completed;

                case DashboardInterviewStatus.Rejected:
                    return Resource.Drawable.dashboard_interview_status_rejected;
            }

            throw new ArgumentException("status is unknown - {0}".FormatString(status));
        }
    }
}