using System.Collections.Generic;
using Android.App;
using Android.Content;

namespace CAPI.Android.Controls.Navigation
{
    public class NavigationListener : Java.Lang.Object, ActionBar.IOnNavigationListener
    {
        private bool synthetic = true;

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
                if (synthetic)
                {
                    synthetic = false;
                    return true;
                }
                return this.items[itemPosition].Handle(this);
            }
            catch
            {
                return false;
            }
        }
    }
}