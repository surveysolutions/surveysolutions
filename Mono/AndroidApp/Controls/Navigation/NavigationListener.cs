using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace AndroidApp.Controls.Navigation
{
    public class NavigationListener : Java.Lang.Object, ActionBar.IOnNavigationListener
    {
        private readonly Context context;
        private readonly IList<NavigationItem> items;
        public NavigationListener(Context context, IList<NavigationItem> items)
        {
            this.context = context;
            this.items = items;
        }

        public bool OnNavigationItemSelected(int itemPosition, long itemId)
        {
            try
            {
                return items[itemPosition].Handle(this);
            }catch
            {
                return false;
            }
        }
    }
}