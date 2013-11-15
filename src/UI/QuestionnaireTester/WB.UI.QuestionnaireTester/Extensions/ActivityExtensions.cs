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

            var logoutButton = new Button(activity);
            logoutButton.Text = "Log out";
            logoutButton.Click += (s, e) =>
            {
                CapiTesterApplication.Membership.LogOff();
            };
            actionBar.SetCustomView(logoutButton,
                new ActionBar.LayoutParams(ActionBar.LayoutParams.WrapContent, ActionBar.LayoutParams.FillParent, GravityFlags.Right));         
        }
    }
}