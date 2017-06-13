using Android.Support.Transitions;
using Android.Support.V7.Widget;
using Android.Views;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.Support.V7.RecyclerView;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;

namespace WB.UI.Interviewer.Activities.Dashboard
{
    public class RecyclerViewAdapter : MvxRecyclerAdapter
    {
        private readonly RecyclerView recyclerView;
        private readonly IMvxAndroidBindingContext bindingContext;

        public RecyclerViewAdapter(RecyclerView recyclerView, IMvxAndroidBindingContext bindingContext)
        {
            this.recyclerView = recyclerView;
            this.bindingContext = bindingContext;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemBindingContext = new MvxAndroidBindingContext(parent.Context, this.bindingContext.LayoutInflaterHolder);
            var vh = new ExpandableViewHolder(this.InflateViewForHolder(parent, viewType, itemBindingContext), itemBindingContext)
            {
                Click = this.ItemClick,
                LongClick = this.ItemLongClick
            };

            return vh;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            base.OnBindViewHolder(holder, position);

            var viewModel = (IDashboardItem)this.GetItem(position);

            if (viewModel.HasExpandedView)
            {
                var viewHolder = (ExpandableViewHolder)holder;

                viewHolder.DashboardItem.Click += (sender, args) =>
                {
                    bool shouldExpand = viewHolder.DetailsView.Visibility == ViewStates.Gone;

                    ChangeBounds transition = new ChangeBounds();
                    transition.SetDuration(125);

                    if (shouldExpand)
                    {
                        viewHolder.DetailsView.Visibility = ViewStates.Visible;
                        viewHolder.ExpandHandle.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        viewHolder.DetailsView.Visibility = ViewStates.Gone;
                        viewHolder.ExpandHandle.Visibility = ViewStates.Visible;
                    }

                    TransitionManager.BeginDelayedTransition(this.recyclerView, transition);
                    viewHolder.DashboardItem.Activated = shouldExpand;
                };
            }
        }
    }
}