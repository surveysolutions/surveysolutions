using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Esri.ArcGISRuntime.UI.Controls;
using MvvmCross.Binding.BindingContext;
using MvvmCross.WeakSubscription;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Enumerator.Activities.Callbacks;
using WB.UI.Shared.Extensions.Entities;
using WB.UI.Shared.Extensions.ViewModels;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WB.UI.Shared.Extensions.Activities
{
    [Activity(WindowSoftInputMode = SoftInput.StateHidden, 
        Theme = "@style/AppTheme", 
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize,
        Exported = false)]
    public class GeographyEditorActivity : MapsBaseActivity<GeographyEditorViewModel>
    {
        protected override int ViewResourceId => Resource.Layout.interview_area_editor;

        public static Action<AreaEditorResult> OnAreaEditCompleted;

        private MvxWeakEventSubscription<ImageButton> clickedMenuButton;
        private MvxWeakEventSubscription<ImageButton> clickedZoomMenuButton;

        private void Cancel()
        {
            var handler = OnAreaEditCompleted;
            handler?.Invoke(null);
            this.Finish();
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.ViewModel.MapView = this.FindViewById<MapView>(Resource.Id.map_view);
            
            this.ViewModel.OnAreaEditCompleted = OnAreaEditCompleted;
            
            var buttonMenu = this.FindViewById<ImageButton>(Resource.Id.butMenu);
            clickedMenuButton = buttonMenu.WeakSubscribe(nameof(buttonMenu.Click), this.ClickedMenuButton);
        
            var buttonEye = this.FindViewById<ImageButton>(Resource.Id.butZoomMenu);
            clickedZoomMenuButton = buttonEye.WeakSubscribe(nameof(buttonEye.Click), this.ClickedZoomButton);
        }

        protected override bool BackButtonCustomAction => true;
        protected override void BackButtonPressed()
        {
            Cancel();
        }
        
        private void ClickedZoomButton(object sender, EventArgs e)
        {
            ShowPopupMenu((ImageButton)sender, [
                new CustomMenuItem(UIResources.MenuItem_Title_ZoomMap, () => this.ViewModel.ShowFullMapCommand.Execute(), Resource.Drawable.icon_zoom_map),
                new CustomMenuItem(UIResources.MenuItem_Title_ZoomShapefile, () => this.ViewModel.ShowShapefileCommand.Execute(), Resource.Drawable.icon_zoom_everything),
                new CustomMenuItem(UIResources.MenuItem_Title_ZoomCollectedData, () => this.ViewModel.ShowAllItemsCommand.Execute(), Resource.Drawable.icon_zoom_collected_data),
            ]);
        }

        private void ClickedMenuButton(object sender, EventArgs e)
        {
            List<CustomMenuItem> customMenuItems = new();
            customMenuItems.Add(new CustomMenuItem(UIResources.MenuItem_Title_ChangeMap, () => this.ViewModel.SwitchMapCommand.Execute(), Resource.Drawable.icon_change_map));
            customMenuItems.Add(new CustomMenuItem(UIResources.MenuItem_Title_ChangeShapefile, () => this.ViewModel.SwitchShapefileCommand.Execute(), Resource.Drawable.icon_change_shapefile));
                
            customMenuItems.Add(this.ViewModel.IsLocationEnabled
                ? new CustomMenuItem(UIResources.MenuItem_Title_HideLocation, () => this.ViewModel.SwitchLocatorCommand.Execute(), Resource.Drawable.icon_location)
                : new CustomMenuItem(UIResources.MenuItem_Title_ShowLocation, () => this.ViewModel.SwitchLocatorCommand.Execute(), Resource.Drawable.icon_location));
            
            customMenuItems.Add(new CustomMenuItem(UIResources.MenuItem_Title_AreaCancelEdit, () => this.ViewModel.CancelCommand.Execute(), Resource.Drawable.icon_exit));

            ShowPopupMenu((ImageButton)sender, customMenuItems);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.ViewModel.OnAreaEditCompleted = null;
                
                clickedMenuButton?.Dispose();
                clickedMenuButton = null;
                clickedZoomMenuButton?.Dispose();
                clickedZoomMenuButton = null;
            }

            base.Dispose(disposing);
        }
    }
}
