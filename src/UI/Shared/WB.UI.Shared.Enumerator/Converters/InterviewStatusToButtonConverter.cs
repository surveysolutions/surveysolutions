using System;
using System.Globalization;
using MvvmCross.Converters;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;

namespace WB.UI.Shared.Enumerator.Converters
{
    public class InterviewStatusToButtonConverter : MvxValueConverter<DashboardInterviewStatus, int>
    {
        protected override int Convert(DashboardInterviewStatus status, Type targetType, object parameter, CultureInfo culture)
        {
            switch (status)
            {
                case DashboardInterviewStatus.Assignment:
                    return Resource.Color.dashboard_assignment_create;
                case DashboardInterviewStatus.New:
                    return Resource.Color.group_started;

                case DashboardInterviewStatus.InProgress:
                    return Resource.Color.group_started;

                case DashboardInterviewStatus.Completed:
                    return Resource.Color.group_completed;

                case DashboardInterviewStatus.Rejected:
                    return Resource.Color.group_with_invalid_answers;
                case DashboardInterviewStatus.ApprovedBySupervisor:
                    return Resource.Color.dashboard_approved_by_supervisor;
                case DashboardInterviewStatus.RejectedByHeadquarters:
                    return Resource.Color.dashboard_rejected_by_headquarter;
            }

            return Resource.Drawable.default_input_button;
        }
    }
}
