using Android.Support.Transitions;
using Android.Support.V7.Widget;
using Android.Views;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.Support.V7.RecyclerView;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;

namespace WB.UI.Interviewer.Activities
{
    public class DashboardRecyclerViewAdapter : MvxRecyclerAdapter
    {
        private readonly RecyclerView recyclerView;
        private readonly IMvxAndroidBindingContext bindingContext;

        public DashboardRecyclerViewAdapter(RecyclerView recyclerView, IMvxAndroidBindingContext bindingContext)
        {
            this.recyclerView = recyclerView;
            this.bindingContext = bindingContext;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemBindingContext = new MvxAndroidBindingContext(parent.Context, this.bindingContext.LayoutInflaterHolder);
            var vh = new DashboardExpandableViewHolder(this.InflateViewForHolder(parent, viewType, itemBindingContext), itemBindingContext)
            {
                Click = this.ItemClick,
                LongClick = this.ItemLongClick
            };

            return vh;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            base.OnBindViewHolder(holder, position);

            var viewHolder = (DashboardExpandableViewHolder)holder;
            var viewModel = (IDashboardItem)GetItem(position);

            if (viewModel.HasExpandedView)
            {
                viewHolder.DashboardItem.Click += (sender, args) =>
                {
                    bool shouldExpand = viewHolder.DetailsView.Visibility == ViewStates.Gone;

                    ChangeBounds transition = new ChangeBounds();
                    transition.SetDuration(125);

                    if (shouldExpand)
                    {
                        viewHolder.DetailsView.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        viewHolder.DetailsView.Visibility = ViewStates.Gone;
                    }

                    TransitionManager.BeginDelayedTransition(this.recyclerView, transition);
                    viewHolder.DashboardItem.Activated = shouldExpand;
                };
            }
        }
    }
}