using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.Widget;
using Esri.ArcGISRuntime.UI.Controls;
using MvvmCross.WeakSubscription;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Shared.Extensions.ViewModels;
using PopupMenu = AndroidX.AppCompat.Widget.PopupMenu;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WB.UI.Shared.Extensions.Activities;

[Activity(WindowSoftInputMode = SoftInput.StateHidden,
    Theme = "@style/AppTheme",
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize,
    Exported = false)]
public class AssignmentMapActivity : MarkersMapActivity<AssignmentMapViewModel, AssignmentMapViewModelArgs>
{
    protected override int ViewResourceId => Resource.Layout.assignment_map;
    
    protected override bool BackButtonCustomAction => true;
    protected override void BackButtonPressed()
    {
        this.ViewModel.NavigateToDashboardCommand.Execute();
        this.Finish();
    }
    
    private MvxWeakEventSubscription<ImageButton> clickedMenuButton;
    private MvxWeakEventSubscription<ImageButton> clickedEyeButton;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        this.ViewModel.MapView = this.FindViewById<MapView>(Resource.Id.map_view);
        
        var buttonMenu = this.FindViewById<ImageButton>(Resource.Id.butMenu);
        clickedMenuButton = buttonMenu.WeakSubscribe(nameof(buttonMenu.Click), this.ClickedMenuButton);
        
        var buttonEye = this.FindViewById<ImageButton>(Resource.Id.butEye);
        clickedEyeButton = buttonEye.WeakSubscribe(nameof(buttonEye.Click), this.ClickedEyeButton);
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

    private void ClickedEyeButton(object sender, EventArgs e)
    {
        ShowPopupMenu((ImageButton)sender, [
            new CustomMenuItem("Zoom all map", () => this.ViewModel.ShowFullMapCommand.Execute()),
            new CustomMenuItem("Zoom all collected data", () => this.ViewModel.ShowAllItemsCommand.Execute()),
            new CustomMenuItem("Zoom all tracking data", () => this.ViewModel.ShowFullMapCommand.Execute())
        ]);
    }

    private void ClickedMenuButton(object sender, EventArgs e)
    {
        ShowPopupMenu((ImageButton)sender, [
            new CustomMenuItem("Change Map", () => this.ViewModel.LoadShapefile.Execute()),
            //new CustomMenuItem("Change Boundaries", () => this.ViewModel.LoadShapefile.Execute()),
            new CustomMenuItem(UIResources.MenuItem_Title_GeoTracking, () => this.ViewModel.StartGeoTrackingCommand.Execute()),
            new CustomMenuItem(UIResources.MenuItem_Title_Geofencing, () => this.ViewModel.StartGeofencingCommand.Execute()),
            new CustomMenuItem("Exit to dashboard", () => this.ViewModel.NavigateToDashboardCommand.Execute()),
        ]);
    }

    private void ShowPopupMenu(View anchorView, CustomMenuItem[] menuActions)
    {
        var context = new ContextThemeWrapper(this, Resource.Style.CustomPopupMenu);
        PopupMenu popupMenu = new PopupMenu(context, anchorView);
        
        for (int i = 0; i < menuActions.Length; i++)
        {
            var customMenuItem = menuActions[i];
            var menuItem = popupMenu.Menu.Add(0, i, i, customMenuItem.Title);
            if (customMenuItem.IconResId.HasValue)
                menuItem.SetIcon(customMenuItem.IconResId.Value);
            
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

    public override bool OnCreateOptionsMenu(IMenu menu)
    {
        this.MenuInflater.Inflate(Resource.Menu.geofencing, menu);

        menu.LocalizeMenuItem(Resource.Id.menu_geofencing, UIResources.MenuItem_Title_Geofencing);
        menu.LocalizeMenuItem(Resource.Id.menu_geo_tracking, UIResources.MenuItem_Title_GeoTracking);

        return base.OnCreateOptionsMenu(menu);
    }
    
    public override bool OnOptionsItemSelected(IMenuItem item)
    {
        if(item.ItemId == Resource.Id.menu_geofencing)
            this.ViewModel.StartGeofencingCommand.Execute();
        if(item.ItemId == Resource.Id.menu_geo_tracking)
            this.ViewModel.StartGeoTrackingCommand.Execute();
            
        return base.OnOptionsItemSelected(item);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            clickedMenuButton?.Dispose();
            clickedMenuButton = null;
            
            clickedEyeButton?.Dispose();
            clickedEyeButton = null;
        }
        
        base.Dispose(disposing);
    }
}
