using Android.App;
using Android.Views;
using CAPI.Android.Controls.Navigation;

namespace CAPI.Android.Extensions
{
    public static class ActivityExtensions
    {
        public static void CreateActionBar( this Activity activity)
        {
            activity.ActionBar.NavigationMode = ActionBarNavigationMode.List;
            activity.ActionBar.DisplayOptions = ActionBarDisplayOptions.ShowHome;

            NavigationItemsCollection navigation = new NavigationItemsCollection(activity);
            
            activity.ActionBar.SetListNavigationCallbacks(
                new NavigationSpinnerAdapter(activity, navigation),
                new NavigationListener(activity, navigation));
            
            if (navigation.SelectedItemIndex != null)
                activity.ActionBar.SetSelectedNavigationItem(navigation.SelectedItemIndex.Value);
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