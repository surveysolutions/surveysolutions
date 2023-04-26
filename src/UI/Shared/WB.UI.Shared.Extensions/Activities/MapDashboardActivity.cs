using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.ConstraintLayout.Helper.Widget;
using AndroidX.DrawerLayout.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.ViewPager.Widget;
using AndroidX.ViewPager2.Widget;
using Esri.ArcGISRuntime.UI.Controls;
using Google.Android.Material.Divider;
using Java.Lang;
using MvvmCross.DroidX.RecyclerView;
using MvvmCross.DroidX.RecyclerView.ItemTemplates;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.WeakSubscription;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Enumerator.Activities.Callbacks;
using WB.UI.Shared.Extensions.Activities.Carousel;
using WB.UI.Shared.Extensions.ViewModels;
using Toolbar=AndroidX.AppCompat.Widget.Toolbar;

namespace WB.UI.Shared.Extensions.Activities
{
    public abstract class MapDashboardActivity<T> : BaseActivity<T> where T : MapDashboardViewModel
    {
        protected override int ViewResourceId => Resource.Layout.map_dashboard;

        private DrawerLayout drawerLayout;
        private ActionBarDrawerToggle drawerToggle;

        private IDisposable onDrawerOpenedSubscription;
        private IDisposable onMapViewMapTappedSubscription;

        public Toolbar Toolbar { get; private set; }

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

            onDrawerOpenedSubscription = this.drawerLayout.WeakSubscribe<DrawerLayout, DrawerLayout.DrawerOpenedEventArgs>(
                nameof(this.drawerLayout.DrawerOpened),
                OnDrawerLayoutOnDrawerOpened);

            // var carousel = this.FindViewById<Carousel>(Resource.Id.markers_carusel);
            // carousel.SetAdapter();

            var markerRecyclerView = (MvxRecyclerView) this.FindViewById(Resource.Id.markers_recycler_view);
            markerRecyclerView.HasFixedSize = true;

            var horizontalLayout = new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false);
            markerRecyclerView.SetLayoutManager(horizontalLayout);
            //var layoutManager = new LinearLayoutManager(this);
            //markerRecyclerView.SetLayoutManager(layoutManager);
            markerRecyclerView.SetItemAnimator(new DefaultItemAnimator());

            //var adapter = new MarkersRecyclerViewAdapter((IMvxAndroidBindingContext)base.BindingContext);
            //markerRecyclerView.SetAdapter(adapter);




            var viewPager = this.FindViewById<ViewPager2>(Resource.Id.carousel_view_pager);

            //viewPager.ClipChildren = false;
            //viewPager.ClipToPadding = false;
            //viewPager.OffscreenPageLimit = 3;
            //viewPager.OverScrollMode = OverScrollMode.Never;
            
            var adapter = new MarkersRecyclerViewAdapter((IMvxAndroidBindingContext)base.BindingContext);
            adapter.ItemTemplateSelector = new MvxDefaultTemplateSelector(Resource.Layout.marker_card);
            adapter.ItemsSource = ViewModel.AvailableMarkers;
            viewPager.Adapter = adapter;
            
            var bindingSet = this.CreateBindingSet();
            bindingSet.Bind(adapter)
                .For(v => v.ItemsSource)
                .To(vm => vm.AvailableMarkers);
            bindingSet.Apply();

            /*var compositePageTransformer = new CompositePageTransformer();
            compositePageTransformer.AddTransformer(
                new MarginPageTransformer((int)(40 * Resources.DisplayMetrics.Density)));
            viewPager.SetPageTransformer(compositePageTransformer);*/

            viewPager.OffscreenPageLimit = 1;

            var pageTransformer = new CarouselIPageTransformer();
            viewPager.SetPageTransformer(pageTransformer);
            var itemDecoration = new HorizontalMarginItemDecoration(
                this,
                Resource.Dimension.carousel_current_item_horizontal_margin
            );
            viewPager.AddItemDecoration(itemDecoration);
            
            
            
            
            
            this.ViewModel.MapView = this.FindViewById<MapView>(Resource.Id.map_view);
            onMapViewMapTappedSubscription = this.ViewModel.MapView.WeakSubscribe<MapView, GeoViewInputEventArgs>(
                nameof(this.ViewModel.MapView.GeoViewTapped),
                this.ViewModel.OnMapViewTapped);
            
            System.Threading.Tasks.Task.Run(() => this.ViewModel.MapControlCreatedAsync());
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

        protected override void OnDestroy()
        {
            onDrawerOpenedSubscription?.Dispose();
            onMapViewMapTappedSubscription?.Dispose();

            base.OnDestroy();
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
