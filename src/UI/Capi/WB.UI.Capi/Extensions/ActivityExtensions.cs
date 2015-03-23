using Android.App;
using Android.Widget;
using WB.UI.Capi.Controls.Navigation;

namespace WB.UI.Capi.Extensions
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
                activity.Finish();
                return true;
            }
            return false;
        }
    }
}