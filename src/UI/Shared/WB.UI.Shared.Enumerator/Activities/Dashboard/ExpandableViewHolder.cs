using Android.Views;
using AndroidX.Transitions;
using MvvmCross.DroidX.RecyclerView;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.WeakSubscription;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;

namespace WB.UI.Shared.Enumerator.Activities.Dashboard
{
    public class ExpandableViewHolder : MvxRecyclerViewHolder
    {
        private IDisposable dashboardItemClickSubscription, menuHandleClickSubscription;

        public ViewGroup DashboardItem { get; private set; }
        public ImageView MenuHandle { get; private set;}
        
        public ExpandableViewHolder(View itemView, IMvxAndroidBindingContext context) : base(itemView, context)
        {
            this.DashboardItem = itemView.FindViewById<ViewGroup>(Resource.Id.dashboardItem);
            this.MenuHandle = itemView.FindViewById<ImageView>(Resource.Id.menu);
        }

        public override void OnAttachedToWindow()
        {
            if (dashboardItemClickSubscription == null)
            {
                if(DashboardItem != null)
                    dashboardItemClickSubscription = DashboardItem.WeakSubscribe(nameof(View.Click), CardClick);
            }

            if (menuHandleClickSubscription == null)
            {
                if(MenuHandle != null)
                    menuHandleClickSubscription = MenuHandle.WeakSubscribe(nameof(View.Click), MenuClick);
            }
            
            base.OnAttachedToWindow();
        }

        public override void OnDetachedFromWindow()
        {
            dashboardItemClickSubscription?.Dispose();
            dashboardItemClickSubscription = null;
            menuHandleClickSubscription?.Dispose();
            menuHandleClickSubscription = null;
            
            base.OnDetachedFromWindow();
        }
        
        public override void OnViewRecycled()
        {
            MenuHandle = null;
            DashboardItem = null;
            
            base.OnViewRecycled();
        }

        public void CardClick(object o, EventArgs eventArgs)
        {
            if (GetItem() is IDashboardItem dashboardItemModel)
            {
                DashboardItem.ClearAnimation();

                var transition = new ChangeBounds();
                transition.SetDuration(125);
                TransitionManager.BeginDelayedTransition(DashboardItem, transition);

                dashboardItemModel.IsExpanded = !dashboardItemModel.IsExpanded;
            }
        }

        public void MenuClick(object o, EventArgs eventArgs)
        {
            if (GetItem() is IDashboardViewItem dashboardItem)
            {
                var popup = new PopupMenu(this.DashboardItem.Context, this.MenuHandle, GravityFlags.Left);
                var actions = dashboardItem.ContextMenu.Where(a => a.Command.CanExecute());
                foreach (var action in actions)
                {
                    var menu = popup.Menu.Add(action.Label);
                    action.Tag = menu.GetHashCode();
                }

                popup.MenuItemClick += (s, e) =>
                    {
                        var action = dashboardItem.ContextMenu.SingleOrDefault(a => a.Tag == e.Item.GetHashCode());
                        action?.Command?.Execute();
                    };

                popup.Show();
            }
        }
        
        public Func<object> GetItem { get; set; }
    }
}
