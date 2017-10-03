using System;
using MvvmCross.Droid.Support.V7.RecyclerView.ItemTemplates;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;

namespace WB.UI.Interviewer.CustomControls
{
    public class InterviewerDashboardTemplateSelector : IMvxTemplateSelector
    {
        private static readonly Type CensusType = typeof(CensusQuestionnaireDashboardItemViewModel);
        private static readonly Type InterviewType = typeof(InterviewDashboardItemViewModel);
        private static readonly Type AssignmentType = typeof(AssignmentDashboardItemViewModel);
        private static readonly Type SubtitleType = typeof(DashboardSubTitleViewModel);

        public int GetItemViewType(object forItemObject)
        {
            var typeOfViewModel = forItemObject.GetType();

            if (typeOfViewModel == CensusType)
                return Resource.Layout.dashboard_census_questionnare_item;

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
    }
}