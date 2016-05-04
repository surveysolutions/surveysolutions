using Android.Views;

namespace WB.UI.Shared.Enumerator.Activities
{
    public static class MenuLocalizationHelper
    {
        public static void LocalizeMenuItem(this IMenu menu, int menuItemId, string menuItemLocalizedTitle)
        {
            var menuItem = menu.FindItem(menuItemId);
            if (menuItem != null)
            {
                menuItem.SetTitle(menuItemLocalizedTitle);
            }
        }
    }
}