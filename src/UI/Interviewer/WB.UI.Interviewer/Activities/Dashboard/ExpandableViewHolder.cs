using System;
using Android.Content;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.Support.V7.RecyclerView;

namespace WB.UI.Interviewer.Activities.Dashboard
{
    public class ExpandableViewHolder : MvxRecyclerViewHolder
    {
        public ViewGroup DashboardItem { get; }
        public ImageView MenuHandle { get; }


        public ExpandableViewHolder(View itemView, IMvxAndroidBindingContext context) : base(itemView, context)
        {
            this.DashboardItem = itemView.FindViewById<ViewGroup>(Resource.Id.dashboardItem);
            
            if (this.DashboardItem != null)
            {
                this.DashboardItem.Click += (sender, args) => this.OnCardClick();
            }

            this.MenuHandle = itemView.FindViewById<ImageView>(Resource.Id.menu);

            if(MenuHandle != null)
            {
                this.MenuHandle.Click += (sender, args) => this.OnMenuClick(itemView);
            }
        }

        public Action<ExpandableViewHolder> CardClick;

        public Action<ExpandableViewHolder, Context> MenuClick;

        protected virtual void OnCardClick() => this.CardClick?.Invoke(this);        

        protected virtual void OnMenuClick(View itemView) => this.MenuClick?.Invoke(this, itemView.Context);
    }
}
