using System;
using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.Support.RecyclerView;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;

namespace WB.UI.Interviewer.CustomControls
{
    public class InterviewerDashboardAdapter : MvxRecyclerAdapter
    {
        private const int UnknownViewType = -1;

        private static readonly Dictionary<Type, int> EntityTemplates = new Dictionary<Type, int>
        {
            {typeof (CensusQuestionnaireDashboardItemViewModel), Resource.Layout.dashboard_census_questionnare_item },
            {typeof (InterviewDashboardItemViewModel), Resource.Layout.dashboard_interview_item },
        };

        public InterviewerDashboardAdapter(IMvxAndroidBindingContext bindingContext)
           : base(bindingContext)
        {
        }

        public override int GetItemViewType(int position)
        {
            object source = this.GetItem(position);
            var typeOfViewModel = source.GetType();
            return EntityTemplates.ContainsKey(typeOfViewModel) ? EntityTemplates[typeOfViewModel] : UnknownViewType;
        }

        protected override View InflateViewForHolder(ViewGroup parent, int viewType, IMvxAndroidBindingContext bindingContext)
        {
            return viewType != UnknownViewType
                ? bindingContext.BindingInflate(viewType, parent, false)
                : this.CreateEmptyView(parent.Context);
        }

        private View CreateEmptyView(Context context)
        {
            return new View(context);
        }
    }

}