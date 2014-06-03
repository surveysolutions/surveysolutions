using Android.App;
using Android.Views;
using Android.Widget;
using Microsoft.Practices.ServiceLocation;
using WB.UI.Shared.Android.Controls.ScreenItems;

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

            var imgProgress = (ImageView)actionBar.CustomView.FindViewById(Resource.Id.imgAnswerProgress);
            var progressBarProvider = ServiceLocator.Current.GetInstance<IAnswerProgressIndicator>();
            progressBarProvider.Setup(
                show: () => activity.RunOnUiThread(() => imgProgress.Visibility = ViewStates.Visible),
                hide: () => activity.RunOnUiThread(() => imgProgress.Visibility = ViewStates.Invisible));
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