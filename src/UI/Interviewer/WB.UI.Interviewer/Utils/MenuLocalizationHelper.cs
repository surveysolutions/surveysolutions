using Android.Views;
using WB.Core.BoundedContexts.Interviewer.Properties;

namespace WB.UI.Interviewer.Utils
{
    public static class MenuLocalizationHelper
    {
        public static void LocalizeMenuItems(this IMenu menu)
        {
            if (menu == null)
                return;

            LocalizeMenuItem(menu, Resource.Id.menu_login, InterviewerUIResources.MenuItem_Title_Login);
            LocalizeMenuItem(menu, Resource.Id.menu_dashboard, InterviewerUIResources.MenuItem_Title_Dashboard);
            LocalizeMenuItem(menu, Resource.Id.menu_signout, InterviewerUIResources.MenuItem_Title_SignOut);
            LocalizeMenuItem(menu, Resource.Id.menu_settings, InterviewerUIResources.MenuItem_Title_Settings);
            LocalizeMenuItem(menu, Resource.Id.menu_troubleshooting, InterviewerUIResources.MenuItem_Title_Troubleshooting);
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