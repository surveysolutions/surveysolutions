using System;
using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.UI.Shared.Enumerator.CustomControls;


namespace WB.UI.Interviewer.CustomControls
{
    public class InterviewerDashboardAdapter : MvxRecyclerViewAdapter
    {
        private const int UnknownViewType = -1;

        public InterviewerDashboardAdapter(Context context, IMvxAndroidBindingContext bindingContext)
            : base(context, bindingContext)
        {
        }

        private static readonly Dictionary<Type, int> EntityTemplates = new Dictionary<Type, int>
        {
            {typeof (CensusQuestionnaireDashboardItemViewModel), Resource.Layout.dashboard_census_questionnare_item },
            {typeof (InterviewDashboardItemViewModel), Resource.Layout.dashboard_interview_item },
        };

        public override int GetItemViewType(int position)
        {
            object source = this.GetRawItem(position);
            var typeOfViewModel = source.GetType();
            return EntityTemplates.ContainsKey(typeOfViewModel) ? EntityTemplates[typeOfViewModel] : UnknownViewType;
        }

        protected override View InflateViewForHolder(ViewGroup parent, int viewType, IMvxAndroidBindingContext bindingContext)
        {
            return viewType != UnknownViewType
                ? bindingContext.BindingInflate(viewType, parent, false)
                : this.CreateEmptyView();
        }

        private View CreateEmptyView()
        {
            return new View(this.Context);
        }
    }

}