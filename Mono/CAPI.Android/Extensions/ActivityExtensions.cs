using System;
using System.Security.Authentication;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using CAPI.Android.Controls.Navigation;

namespace CAPI.Android.Extensions
{
    public static class ActivityExtensions
    {
        public static void CreateActionBar(this Activity activity)
        {
            // Set up your ActionBar
            ActionBar actionBar = activity.ActionBar;
            actionBar.SetDisplayShowHomeEnabled(false);
            actionBar.SetDisplayShowTitleEnabled(false);
            actionBar.SetDisplayShowCustomEnabled(true);
            actionBar.SetDisplayUseLogoEnabled(true);
            actionBar.SetCustomView(Resource.Layout.ActionBar);

            // You customization
            var greetingsTextView = (TextView)actionBar.CustomView.FindViewById(Resource.Id.greetingsTextView);

            greetingsTextView.Text = CapiApplication.Membership.CurrentUser == null
                ? string.Empty
                : string.Format("Hello, {0}", CapiApplication.Membership.CurrentUser.Name);

            var navigation = new NavigationItemsCollection(activity);
            var pagesSpinner = (Spinner)actionBar.CustomView.FindViewById(Resource.Id.pagesSpinner);
            pagesSpinner.Adapter = new NavigationSpinnerAdapter(navigation);

            pagesSpinner.OnItemSelectedListener= new NavigationListener(navigation);

            if (navigation.SelectedItemIndex != null)
                pagesSpinner.SetSelection(navigation.SelectedItemIndex.Value);            
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

        public static void ClearAllBackStack<T>(this Context context) where T : Activity
        {
            Intent intent = new Intent(context, typeof(T));
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