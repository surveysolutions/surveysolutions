using System;
using System.Globalization;
using MvvmCross.Converters;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;

namespace WB.UI.Shared.Enumerator.Converters
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
