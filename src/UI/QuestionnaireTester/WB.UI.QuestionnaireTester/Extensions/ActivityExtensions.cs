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
                CapiTesterApplication.DesignerMembership.LogOff();
            };

            var tvTitlte = (TextView) actionBar.CustomView.FindViewById(Resource.Id.tvTitlte);
            tvTitlte.Text = activity.Title;
        }

        public static void CreateSearchebleActionBar(this Activity activity)
        {
            // Set up your ActionBar
            ActionBar actionBar = activity.ActionBar;
            actionBar.SetDisplayShowHomeEnabled(false);
            actionBar.SetDisplayShowTitleEnabled(false);
            actionBar.SetDisplayShowCustomEnabled(true);
            actionBar.SetDisplayUseLogoEnabled(true);
            actionBar.SetCustomView(Resource.Layout.ActionBarSearchable);


            var logoutButton = (Button)actionBar.CustomView.FindViewById(Resource.Id.btnLogout);
            logoutButton.Click += (s, e) =>
            {
                CapiTesterApplication.DesignerMembership.LogOff();
            };
        }
    }
}