﻿using Android.OS;
using Android.Views;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.Support.V7.RecyclerView;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Interviewer.Activities.Dashboard
{
    public abstract class RecyclerViewFragment<TViewModel> : BaseFragment<ListViewModel<TViewModel>> where TViewModel : class
    {
        protected override int ViewResourceId => Resource.Layout.fragment_dashboard_tab;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = this.BindingInflate(this.ViewResourceId, null);

            var recyclerView = view.FindViewById<MvxRecyclerView>(Resource.Id.dashboard_tab_recycler);
            if (recyclerView != null)
                recyclerView.Adapter = new RecyclerViewAdapter(recyclerView, (IMvxAndroidBindingContext)base.BindingContext);

            return view;
        }
    }
}