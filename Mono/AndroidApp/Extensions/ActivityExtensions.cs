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
using AndroidApp.Controls.Navigation;

namespace AndroidApp.Extensions
{
    public static class ActivityExtensions
    {
        public static void CreateActionBar( this Activity activity)
        {
            activity.ActionBar.NavigationMode = ActionBarNavigationMode.List;
            IList<NavigationItem> navigation = new NavigationItemsCollection(activity);
            activity.ActionBar.SetListNavigationCallbacks(
                new NavigationSpinnerAdapter(activity, navigation),
                new NavigationListener(activity, navigation));
        }
        public static void EnableDisableView(this View view, bool enabled)
        {
            view.Enabled = enabled;
            ViewGroup group = view as ViewGroup;
            if (group != null)
            {

                for (int idx = 0; idx < group.ChildCount; idx++)
                {
                    EnableDisableView(group.GetChildAt(idx), enabled);
                }
            }

        }
    }
}