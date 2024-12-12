using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.CardView.Widget;
using AndroidX.ConstraintLayout.Widget;
using AndroidX.DrawerLayout.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.ViewPager2.Widget;
using Esri.ArcGISRuntime.UI.Controls;
using MvvmCross.DroidX.RecyclerView;
using MvvmCross.DroidX.RecyclerView.ItemTemplates;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.WeakSubscription;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Extensions.Activities.Carousel;
using WB.UI.Shared.Extensions.ViewModels;
using Math = Java.Lang.Math;
using Toolbar=AndroidX.AppCompat.Widget.Toolbar;

namespace WB.UI.Shared.Extensions.Activities
{
    public abstract class MapDashboardActivity<T> : MarkersMapActivity<T, MapDashboardViewModelArgs> where T : MapDashboardViewModel
    {
        protected override int ViewResourceId => Resource.Layout.map_dashboard;

        private DrawerLayout drawerLayout;

        private IDisposable onDrawerOpenedSubscription;
        private IDisposable onImageButtonDrawerOpenSubscription;
        private MvxWeakEventSubscription<ImageButton> clickedMenuButton;
        private MvxWeakEventSubscription<ImageButton> clickedZoomMenuButton;

        private void Cancel()
        {
            this.Finish();
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
            this.drawerLayout = this.FindViewById<DrawerLayout>(Resource.Id.rootLayout);
            ImageButton openDrawerButton = this.FindViewById<ImageButton>(Resource.Id.butBurger);

            onImageButtonDrawerOpenSubscription =
                openDrawerButton.WeakSubscribe<ImageButton>(
                    nameof(openDrawerButton.Click),
                    OnClickImageButtonDrawerClick);

            onDrawerOpenedSubscription =
                this.drawerLayout.WeakSubscribe<DrawerLayout, DrawerLayout.DrawerOpenedEventArgs>(
                    nameof(this.drawerLayout.DrawerOpened),
                    OnDrawerLayoutOnDrawerOpened);
            
            var buttonMenu = this.FindViewById<ImageButton>(Resource.Id.butMenu);
            clickedMenuButton = buttonMenu.WeakSubscribe(nameof(buttonMenu.Click), this.ClickedMenuButton);
        
            var buttonEye = this.FindViewById<ImageButton>(Resource.Id.butZoomMenu);
            clickedZoomMenuButton = buttonEye.WeakSubscribe(nameof(buttonEye.Click), this.ClickedZoomButton);
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
            
            customMenuItems.Add(new CustomMenuItem(UIResources.MenuItem_Title_ExitToDashboard, () => this.ViewModel.NavigateToDashboardCommand.Execute(), Resource.Drawable.icon_exit));

            ShowPopupMenu((ImageButton)sender, customMenuItems);
        }

        private void OnClickImageButtonDrawerClick(object sender, EventArgs e)
        {
            if (!drawerLayout.IsDrawerOpen((int)GravityFlags.Start)) {
                drawerLayout.OpenDrawer((int)GravityFlags.Start);
            }
        }

        protected override bool BackButtonCustomAction => true;
        protected override void BackButtonPressed()
        {
            this.ViewModel.NavigateToDashboardCommand.Execute();
            this.Cancel();
        }

        private void OnDrawerLayoutOnDrawerOpened(object sender, DrawerLayout.DrawerOpenedEventArgs args)
        {
            this.RemoveFocusFromEditText();
            this.HideKeyboard(drawerLayout.WindowToken);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                onDrawerOpenedSubscription?.Dispose();
                onImageButtonDrawerOpenSubscription?.Dispose();
                clickedMenuButton?.Dispose();
                clickedZoomMenuButton?.Dispose();
            }

            base.Dispose(disposing);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if(item.ItemId == Resource.Id.menu_dashboard)
                this.ViewModel.NavigateToDashboardCommand.Execute();
            
            return base.OnOptionsItemSelected(item);
        }
    }
}
