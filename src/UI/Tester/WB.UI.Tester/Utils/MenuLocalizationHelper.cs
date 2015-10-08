using Android.Views;

namespace WB.UI.Tester.Utils
{
    public static class TesterMenuLocalizationHelper
    {
        public static void LocalizeMenuItems(this IMenu menu)
        {
            if (menu == null)
                return;

            //LocalizeMenuItem(menu, Resource.Id.login_settings, TesterUIResources.MenuItem_Title_Settings);
            //LocalizeMenuItem(menu, Resource.Id.interview_dashboard, TesterUIResources.MenuItem_Title_Dashboard);
            //LocalizeMenuItem(menu, Resource.Id.interview_settings, TesterUIResources.MenuItem_Title_Settings);
            //LocalizeMenuItem(menu, Resource.Id.dashboard_signout, TesterUIResources.MenuItem_Title_SignOut);
            //LocalizeMenuItem(menu, Resource.Id.dashboard_settings, TesterUIResources.MenuItem_Title_Settings);
        }

        private static void LocalizeMenuItem(IMenu menu, int menuItemId, string menuItemLocalizedTitle)
        {
            var menuItem = menu.FindItem(menuItemId);
            if (menuItem != null)
            {
                menuItem.SetTitle(menuItemLocalizedTitle);
            }
        }
    }
}