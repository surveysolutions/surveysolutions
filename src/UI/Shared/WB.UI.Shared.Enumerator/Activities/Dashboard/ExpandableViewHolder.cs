using System;
using System.Linq;
using Android.Support.Transitions;
using Android.Views;
using Android.Widget;
using MvvmCross.Droid.Support.V7.RecyclerView;
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
                DashboardItem.Click -= CardClick;
                MenuHandle.Click -= MenuClick;
            }
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

                foreach (var action in dashboardItem.ContextMenu.Where(a => a.Command.CanExecute()))
                {
                    var menu = popup.Menu.Add(action.Label);
                    action.Tag = menu.ItemId;
                }

                popup.MenuItemClick += (s, e) =>
                {
                    var action = dashboardItem.ContextMenu.SingleOrDefault(a => a.Tag == e.Item.ItemId);
                    action.Command.Execute();
                };

                popup.Show();
            }
        }
        
        public Func<object> GetItem { get; set; }
    }
}
