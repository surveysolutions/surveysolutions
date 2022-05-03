using Android.Views;

namespace WB.UI.Shared.Enumerator.Activities;

public static class MenuHelper
{
    public static void VisibleMenuItem(this IMenu menu, int menuItemId, bool visible)
    {
        menu.FindItem(menuItemId)?.SetVisible(visible);
    }
}