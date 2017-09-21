using System.Linq;
using Android.Content;
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
        private readonly IMvxAndroidBindingContext bindingContext;

        public RecyclerViewAdapter(IMvxAndroidBindingContext bindingContext)
        {
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

            var item = this.GetItem(position);

            if(item is IDashboardItem viewModel)
            {
                var viewHolder = (ExpandableViewHolder)holder;
                viewHolder.CardClick = (sender) =>
                {
                    sender.DashboardItem.ClearAnimation();

                    var transition = new ChangeBounds();
                    transition.SetDuration(125);
                    TransitionManager.BeginDelayedTransition(sender.DashboardItem, transition);

                    viewModel.IsExpanded = !viewModel.IsExpanded;
                };
            }

            if(item is IDashboardViewItem viewItem && viewItem.ContextMenu.Any())
            {
                var viewHolder = (ExpandableViewHolder)holder;

                viewHolder.MenuClick = (sender, context) =>
                {
                    var popup = new PopupMenu(context, viewHolder.MenuHandle);

                    foreach(var action in viewItem.ContextMenu.Where(a => a.Command.CanExecute()))
                    {
                        var menu = popup.Menu.Add(action.Label);
                        action.Tag = menu.ItemId;
                    }

                    popup.MenuItemClick += (s, e) =>
                    {
                        var action = viewItem.ContextMenu.SingleOrDefault(a => a.Tag == e.Item.ItemId);
                        action.Command.Execute();
                    };

                    popup.Show();
                };
            }            
        }
        
    }
}