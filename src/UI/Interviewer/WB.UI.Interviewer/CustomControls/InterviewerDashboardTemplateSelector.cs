﻿using System;
using MvvmCross.DroidX.RecyclerView.ItemTemplates;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;

namespace WB.UI.Interviewer.CustomControls
{
    public class InterviewerDashboardTemplateSelector : IMvxTemplateSelector
    {
        private static readonly Type InterviewType = typeof(InterviewDashboardItemViewModel);
        private static readonly Type AssignmentType = typeof(InterviewerAssignmentDashboardItemViewModel);
        private static readonly Type SubtitleType = typeof(DashboardSubTitleViewModel);

        public int GetItemViewType(object forItemObject)
        {
            if (forItemObject == null) return -1;

            var typeOfViewModel = forItemObject.GetType();

            if (typeOfViewModel == InterviewType || typeOfViewModel == AssignmentType)
                return Resource.Layout.dashboard_item;

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
