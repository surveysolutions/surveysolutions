using System;
using System.Collections.Generic;
using MvvmCross.Droid.Support.V7.RecyclerView.ItemTemplates;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;

namespace WB.UI.Interviewer.CustomControls
{
    public class InterviewerDashboardTemplateSelector : IMvxTemplateSelector
    {
        private static readonly Dictionary<Type, int> EntityTemplates = new Dictionary<Type, int>
        {
            {typeof (CensusQuestionnaireDashboardItemViewModel), Resource.Layout.dashboard_census_questionnare_item },
            {typeof (InterviewDashboardItemViewModel), Resource.Layout.dashboard_interview_item },
            {typeof (AssignmentDashboardItemViewModel), Resource.Layout.dashboard_assignment_item },
            {typeof (DashboardSubTitleViewModel), Resource.Layout.dashboard_tab_subtitle }

        };

        public int GetItemViewType(object forItemObject)
        {
            var typeOfViewModel = forItemObject.GetType();
            return EntityTemplates.ContainsKey(typeOfViewModel) ? EntityTemplates[typeOfViewModel] : -1;
        }

        public int GetItemLayoutId(int fromViewType)
        {
            return fromViewType;
        }
    }
}