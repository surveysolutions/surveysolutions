using System;
using Android.Views;

namespace WB.UI.Shared.Enumerator.Activities.Dashboard
{
    public class MenuItemOnMenuItemClickListener : Java.Lang.Object, IMenuItemOnMenuItemClickListener
    {
        private readonly Func<bool> action;

        public MenuItemOnMenuItemClickListener(Func<bool> action)
        {
            this.action = action;
        }

        public bool OnMenuItemClick(IMenuItem item)
        {
            return action.Invoke();
        }
    }
}