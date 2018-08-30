using Android.App;
using Android.Support.Graphics.Drawable;
using Android.Views;

namespace WB.UI.Shared.Enumerator.Activities
{
    public static class MenuLocalizationHelper
    {
        public static void LocalizeMenuItem(this IMenu menu, int menuItemId, string menuItemLocalizedTitle)
        {
            menu.FindItem(menuItemId)?.SetTitle(menuItemLocalizedTitle);
        }
    }
}
