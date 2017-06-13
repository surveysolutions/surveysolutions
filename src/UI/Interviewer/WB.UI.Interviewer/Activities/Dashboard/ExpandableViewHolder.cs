using Android.Views;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.Support.V7.RecyclerView;

namespace WB.UI.Interviewer.Activities.Dashboard
{
    public class ExpandableViewHolder : MvxRecyclerViewHolder
    {
        public View DashboardItem { get; }

        public View DetailsView { get; }

        public ExpandableViewHolder(View itemView, IMvxAndroidBindingContext context) : base(itemView, context)
        {
            this.DetailsView = itemView.FindViewById<View>(Resource.Id.dashboardItemDetails);
            this.DashboardItem = itemView.FindViewById<View>(Resource.Id.dashboardItem);
        }
    }
}