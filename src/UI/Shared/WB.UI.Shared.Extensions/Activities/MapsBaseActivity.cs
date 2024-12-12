using Android.Views;
using AndroidX.Core.Content;
using AndroidX.Core.Graphics.Drawable;
using MvvmCross.ViewModels;
using MvvmCross.WeakSubscription;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Shared.Extensions.Activities;

public abstract class MapsBaseActivity<T> : BaseActivity<T>
    where T : class, IMvxViewModel
{
    protected void ShowPopupMenu(View anchorView, List<CustomMenuItem> menuActions)
    {
        var context = new ContextThemeWrapper(this, Resource.Style.CustomPopupMenu);
        PopupMenu popupMenu = new PopupMenu(context, anchorView);
        
        for (int i = 0; i < menuActions.Count; i++)
        {
            var customMenuItem = menuActions[i];
            var menuItem = popupMenu.Menu.Add(0, i, i, customMenuItem.Title);
            if (customMenuItem.IconResId.HasValue)
            {
                var icon = ContextCompat.GetDrawable(this, customMenuItem.IconResId.Value);
                var tintedIcon = DrawableCompat.Wrap(icon).Mutate();
                DrawableCompat.SetTint(tintedIcon, Resource.Color.map_menu_text);
                menuItem.SetIcon(tintedIcon);
                //menuItem.SetIcon(customMenuItem.IconResId.Value);
            }
            
            
            
            /*var menuItem = popupMenu.Menu.GetItem(i);
            
            var layoutInflater = LayoutInflater.From(this);
            var customView = layoutInflater.Inflate(Resource.Layout.menu_item_custom, null);
            
            var textView = customView.FindViewById<TextView>(Resource.Id.menu_item_text);
            var imageView = customView.FindViewById<ImageView>(Resource.Id.menu_item_icon);
            
            textView.Text = menuActions[i].ToString() + menuActions[i].ToString();
            imageView.SetImageResource(Resource.Drawable.icon_button_default);
            
            menuItem.SetActionView(customView);*/
        }

        popupMenu.WeakSubscribe<PopupMenu, PopupMenu.MenuItemClickEventArgs>(nameof(popupMenu.MenuItemClick), (s, args) =>
        {
            var menuItem = menuActions[args.Item.ItemId];
            menuItem.Action.Invoke();
        });
        
        ForceShowIcons(popupMenu);
        popupMenu.Show();
    }
    
    private void ForceShowIcons(PopupMenu popupMenu)
    {
        try
        {
            var fields = popupMenu.Class.GetDeclaredFields();
            foreach (var field in fields)
            {
                if (field.Name == "mPopup")
                {
                    field.Accessible = true;
                    var mPopup = field.Get(popupMenu);
                    var method = mPopup.Class.GetDeclaredMethod("setForceShowIcon", Java.Lang.Boolean.Type);
                    method.Invoke(mPopup, true);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error enabling icons in PopupMenu: {ex}");
        }
    }
}


public class CustomMenuItem
{
    public CustomMenuItem()
    {
    }

    public CustomMenuItem(string title, Action action): this()
    {
        Title = title;
        Action = action;
    }

    public CustomMenuItem(string title, Action action, int? iconResId) : this(title, action)
    {
        IconResId = iconResId;
    }

    public string Title { get; set; }
    public Action Action { get; set; }
    public int? IconResId { get; set; }
}
