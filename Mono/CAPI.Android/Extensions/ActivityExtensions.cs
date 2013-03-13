using System;
using System.Security.Authentication;
using Android.App;
using Android.Content;
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

        public static bool FinishIfNotLoggedIn(this Activity activity)
        {
            if (!CapiApplication.Membership.IsLoggedIn)
            {
              //  throw new AuthenticationException("invalid credentials");
                activity.Finish();
                return true;
            }
            return false;
        }

        public static void ClearAllBackStack<T>(this Context context) where T:Activity
        {
            Intent intent = new Intent(context,typeof(T) );
            intent.PutExtra("finish", true); // if you are checking for this in your other Activities
            intent.AddFlags(ActivityFlags.ClearTask);
            intent.AddFlags(ActivityFlags.ClearTop);
            intent.AddFlags(ActivityFlags.NewTask);
            context.StartActivity(intent);
        }

        public static void EnableDisableView(this View view, bool enabled)
        {
            bool parentEnabled = true;
            var parentView = view.Parent as View;
            if (parentView != null)
                parentEnabled = parentView.Enabled;
            view.Enabled = parentEnabled && enabled;
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