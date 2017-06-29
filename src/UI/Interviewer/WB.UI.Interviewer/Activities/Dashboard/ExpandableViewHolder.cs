using System;
using Android.Views;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.Support.V7.RecyclerView;

namespace WB.UI.Interviewer.Activities.Dashboard
{
    public class ExpandableViewHolder : MvxRecyclerViewHolder
    {
        public ViewGroup DashboardItem { get; }
        
        public ExpandableViewHolder(View itemView, IMvxAndroidBindingContext context) : base(itemView, context)
        {
            this.DashboardItem = itemView.FindViewById<ViewGroup>(Resource.Id.dashboardItem);
            
            if (this.DashboardItem != null)
            {
                this.DashboardItem.Click += (sender, args) => this.OnCardClick();
            }
        }

        public Action<ExpandableViewHolder> CardClick;

        protected virtual void OnCardClick()
        {
            this.CardClick?.Invoke(this);
        }
    }
}