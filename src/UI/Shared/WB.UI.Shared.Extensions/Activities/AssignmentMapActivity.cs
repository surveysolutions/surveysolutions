using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content;
using AndroidX.Core.Graphics.Drawable;
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
    private MvxWeakEventSubscription<ImageButton> clickedZoomMenuButton;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        this.ViewModel.MapView = this.FindViewById<MapView>(Resource.Id.map_view);
        
        var buttonMenu = this.FindViewById<ImageButton>(Resource.Id.butMenu);
        clickedMenuButton = buttonMenu.WeakSubscribe(nameof(buttonMenu.Click), this.ClickedMenuButton);
        
        var buttonEye = this.FindViewById<ImageButton>(Resource.Id.butZoomMenu);
        clickedZoomMenuButton = buttonEye.WeakSubscribe(nameof(buttonEye.Click), this.ClickedEyeButton);
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
            new CustomMenuItem(UIResources.MenuItem_Title_ZoomMap, () => this.ViewModel.ShowFullMapCommand.Execute(), Resource.Drawable.icon_zoom_map),
            new CustomMenuItem(UIResources.MenuItem_Title_ZoomShapefile, () => this.ViewModel.ShowShapefileCommand.Execute(), Resource.Drawable.icon_zoom_everything),
            new CustomMenuItem(UIResources.MenuItem_Title_ZoomCollectedData, () => this.ViewModel.ShowAllItemsCommand.Execute(), Resource.Drawable.icon_zoom_collected_data),
            new CustomMenuItem(UIResources.MenuItem_Title_ZoomGeoTracking, () => this.ViewModel.ShowTrackingDataCommand.Execute(), Resource.Drawable.icon_zoom_geotracking_data)
        ]);
    }

    private void ClickedMenuButton(object sender, EventArgs e)
    {
        List<CustomMenuItem> customMenuItems = new();
        customMenuItems.Add(new CustomMenuItem(UIResources.MenuItem_Title_CreateInterview, () => this.ViewModel.CreateInterviewCommand.Execute(), Resource.Drawable.icon_create_interview_menu));
        customMenuItems.Add(new CustomMenuItem(UIResources.MenuItem_Title_ChangeMap, () => this.ViewModel.SwitchMapCommand.Execute(), Resource.Drawable.icon_change_map));
            //new CustomMenuItem("Change Boundaries", () => this.ViewModel.LoadShapefile.Execute(), Resource.Drawable.icon_change_map),
            
        if (ViewModel.AllowGeoTracking)
            customMenuItems.Add(this.ViewModel.IsEnabledGeoTracking
                ? new CustomMenuItem(UIResources.MenuItem_Title_StopGeoTracking, () => this.ViewModel.StartGeoTrackingCommand.Execute(), Resource.Drawable.icon_geotracking_default)
                : new CustomMenuItem(UIResources.MenuItem_Title_StartGeoTracking, () => this.ViewModel.StartGeoTrackingCommand.Execute(), Resource.Drawable.icon_geotracking_default));

        if (ViewModel.AllowGeofencing)
            customMenuItems.Add(this.ViewModel.IsEnabledGeofencing
                ? new CustomMenuItem(UIResources.MenuItem_Title_StopGeofencing, () => this.ViewModel.StartGeofencingCommand.Execute(), Resource.Drawable.icon_geofencing_default)
                : new CustomMenuItem(UIResources.MenuItem_Title_StartGeofencing, () => this.ViewModel.StartGeofencingCommand.Execute(), Resource.Drawable.icon_geofencing_default));

        customMenuItems.Add(new CustomMenuItem(UIResources.MenuItem_Title_ExitToDashboard, () => this.ViewModel.NavigateToDashboardCommand.Execute(), Resource.Drawable.icon_exit));

        ShowPopupMenu((ImageButton)sender, customMenuItems);
    }

    private void ShowPopupMenu(View anchorView, List<CustomMenuItem> menuActions)
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
    
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            clickedMenuButton?.Dispose();
            clickedMenuButton = null;
            
            clickedZoomMenuButton?.Dispose();
            clickedZoomMenuButton = null;
        }
        
        base.Dispose(disposing);
    }
}
