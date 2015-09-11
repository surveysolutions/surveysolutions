using System;
using System.Globalization;
using Cirrious.CrossCore.Converters;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.GenericSubdomains.Portable;


namespace WB.UI.Interviewer.Converters
{
    public class InterviewStatusToColorConverter : MvxValueConverter<DashboardInterviewCategories, int>
    {
        protected override int Convert(DashboardInterviewCategories status, Type targetType, object parameter, CultureInfo culture)
        {
            switch (status)
            {
                case DashboardInterviewCategories.New:
                    return Resource.Color.dashboard_new_interview_status;

                case DashboardInterviewCategories.InProgress:
                    return Resource.Color.dashboard_in_progress_tab;

                case DashboardInterviewCategories.Complited:
                    return Resource.Color.dashboard_complited_tab;

                case DashboardInterviewCategories.Rejected:
                    return Resource.Color.dashboard_rejected_tab;
            }

            throw new ArgumentException("status is unknown - {0}".FormatString(status));
        }
    }
}