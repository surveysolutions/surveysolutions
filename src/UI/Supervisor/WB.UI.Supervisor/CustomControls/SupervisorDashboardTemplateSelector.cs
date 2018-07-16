using System;
using MvvmCross.Droid.Support.V7.RecyclerView.ItemTemplates;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Items;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;

namespace WB.UI.Supervisor.CustomControls
{
    public class SupervisorDashboardTemplateSelector : IMvxTemplateSelector
    {
        private static readonly Type InterviewType = typeof(SupervisorDashboardInterviewViewModel);
        private static readonly Type SubtitleType = typeof(DashboardSubTitleViewModel);
        private static readonly Type AssignmentType = typeof(SupervisorAssignmentDashboardItemViewModel);

        public int GetItemViewType(object forItemObject)
        {
            if (forItemObject == null) return -1;

            var typeOfViewModel = forItemObject.GetType();

            if (typeOfViewModel == InterviewType || typeOfViewModel == AssignmentType)
                return Resource.Layout.dashboard_interview_item;

            if (typeOfViewModel == SubtitleType)
                return Resource.Layout.dashboard_tab_subtitle;

            return -1;
        }

        public int GetItemLayoutId(int fromViewType)
        {
            return fromViewType;
        }

        public int ItemTemplateId { get; set; }
    }
}
