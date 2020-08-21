﻿using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using Esri.ArcGISRuntime.UI.Controls;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.UI.Shared.Enumerator.Activities;
using Toolbar = androidx.appcompat.widget.Toolbar;

namespace WB.UI.Shared.Extensions.CustomServices.MapDashboard
{
    [Activity(WindowSoftInputMode = SoftInput.StateHidden,
        Theme = "@style/AppTheme",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize,
        Exported = false)]
    public class MapDashboardActivity : BaseActivity<MapDashboardViewModel>
    {
        protected override int ViewResourceId => Resource.Layout.map_dashboard;

        private DrawerLayout drawerLayout;
        private ActionBarDrawerToggle drawerToggle;

        public Toolbar Toolbar { get; private set; }

        public override void OnBackPressed()
        {
            this.Cancel();
        }

        private void Cancel()
        {
            this.Finish();
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
            this.Toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            this.Toolbar.Title = "";
            this.SetSupportActionBar(this.Toolbar);

            this.drawerLayout = this.FindViewById<DrawerLayout>(Resource.Id.rootLayout);
            this.drawerToggle = new ActionBarDrawerToggle(this, this.drawerLayout, this.Toolbar, 0, 0);
            this.drawerLayout.AddDrawerListener(this.drawerToggle);

            this.drawerToggle.DrawerSlideAnimationEnabled = true;
            this.drawerToggle.DrawerIndicatorEnabled = true;
            this.drawerToggle.SyncState();

            this.drawerLayout.DrawerOpened += (sender, args) =>
            {
                this.RemoveFocusFromEditText();
                this.HideKeyboard(drawerLayout.WindowToken);
            };

            this.ViewModel.MapView = this.FindViewById<MapView>(Resource.Id.map_view);
            this.ViewModel.MapView.GeoViewTapped += this.ViewModel.OnMapViewTapped;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            //this.MenuInflater.Inflate(Resource.Menu.map_dashboard, menu);
            //menu.LocalizeMenuItem(Resource.Id.menu_dashboard, UIResources.MenuItem_Title_Dashboard);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if(item.ItemId == Resource.Id.menu_dashboard)
                this.ViewModel.NavigateToDashboardCommand.Execute();
            
            return base.OnOptionsItemSelected(item);
        }
    }
}
