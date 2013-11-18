using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using WB.UI.Shared.Android.Extensions;

namespace WB.UI.QuestionnaireTester.Extensions
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


            var logoutButton = (Button) actionBar.CustomView.FindViewById(Resource.Id.btnLogout);
            logoutButton.Click += (s, e) =>
            {
                CapiTesterApplication.Membership.LogOff();
            };

            var tvTitlte = (TextView) actionBar.CustomView.FindViewById(Resource.Id.tvTitlte);
            tvTitlte.Text = activity.Title;
        }



        public static bool FinishIfNotLoggedIn(this Activity activity)
        {
            if (!CapiTesterApplication.Membership.IsLoggedIn)
            {
                //  throw new AuthenticationException("invalid credentials");
                activity.Finish();
                return true;
            }
            return false;
        }
    }
}