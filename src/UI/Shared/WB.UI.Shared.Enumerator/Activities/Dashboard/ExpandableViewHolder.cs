using Android.Views;
using AndroidX.Transitions;
using MvvmCross.DroidX.RecyclerView;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;

namespace WB.UI.Shared.Enumerator.Activities.Dashboard
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
                this.DashboardItem.Click += CardClick;
            }

            this.MenuHandle = itemView.FindViewById<ImageView>(Resource.Id.menu);

            if (MenuHandle != null)
            {
                this.MenuHandle.Click += MenuClick;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (DashboardItem != null)
                {
                    DashboardItem.Click -= CardClick;
                    DashboardItem.Dispose();
                }

                if (MenuHandle != null)
                {
                    MenuHandle.Click -= MenuClick;
                    MenuHandle.Dispose();
                }
            }
            base.Dispose(disposing);
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
