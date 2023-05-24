using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.CardView.Widget;
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
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Enumerator.Activities.Callbacks;
using WB.UI.Shared.Enumerator.Activities.Dashboard;
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
        private ViewPager2.OnPageChangeCallback onPageChangeCallback;
        private View.IOnTouchListener onTouchListener;
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

            onDrawerOpenedSubscription =
                this.drawerLayout.WeakSubscribe<DrawerLayout, DrawerLayout.DrawerOpenedEventArgs>(
                    nameof(this.drawerLayout.DrawerOpened),
                    OnDrawerLayoutOnDrawerOpened);

            var viewPager = this.FindViewById<ViewPager2>(Resource.Id.carousel_view_pager);
            onPageChangeCallback = new CarouselOnPageChangeCallback(viewPager);
            viewPager.RegisterOnPageChangeCallback(onPageChangeCallback);
            //viewPager.LayoutChange += ViewPagerOnLayoutChange;
            //viewPager.Click += ViewPagerOnClick;
            
            RecyclerView recyclerView = (RecyclerView) viewPager.GetChildAt(0);
            onTouchListener = new CarouselOnTouchListener();
            recyclerView.SetOnTouchListener(onTouchListener);
            // recyclerView.LayoutParameters = (new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent,
            //     ViewGroup.LayoutParams.WrapContent));
            // recyclerView.ClearOnChildAttachStateChangeListeners();

            var adapter = new CarouselViewAdapter((IMvxAndroidBindingContext)base.BindingContext);
            adapter.ItemTemplateSelector = CreateCarouselTemplateSelector();
            //adapter.ItemTemplateSelector = new MvxDefaultTemplateSelector(Resource.Layout.marker_card);
            adapter.ItemsSource = ViewModel.AvailableMarkers;
            viewPager.Adapter = adapter;

            var bindingSet = this.CreateBindingSet();
            bindingSet.Bind(adapter)
                .For(v => v.ItemsSource)
                .To(vm => vm.AvailableMarkers);
            bindingSet.Apply();


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
        }

        private void ViewPagerOnClick(object sender, EventArgs e)
        {
            var viewPager = (ViewPager2)sender;
            var view = viewPager.FindViewWithTag("position-" + viewPager.CurrentItem);

            view?.Post(() =>
            {
                var wMeasureSpec = View.MeasureSpec.MakeMeasureSpec(view.Width, MeasureSpecMode.Exactly);
                var hMeasureSpec = View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);
                view.Measure(wMeasureSpec, hMeasureSpec);

                if (viewPager?.LayoutParameters != null && viewPager.LayoutParameters.Height != view.MeasuredHeight)
                {
                    viewPager.LayoutParameters.Height = view.MeasuredHeight;
                    viewPager.RequestLayout();
                }
            });
        }

        private void ViewPagerOnLayoutChange(object sender, View.LayoutChangeEventArgs e)
        {
            var viewPager = (ViewPager2)sender;
            var view = viewPager.FindViewWithTag("position-" + viewPager.CurrentItem);
            var cardView = view?.FindViewById<CardView>(Resource.Id.dashboardItem);

            cardView?.Post(() =>
            {
                var wMeasureSpec = View.MeasureSpec.MakeMeasureSpec(cardView.Width, MeasureSpecMode.Exactly);
                var hMeasureSpec = View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);
                cardView.Measure(wMeasureSpec, hMeasureSpec);

                if (viewPager?.LayoutParameters != null && viewPager.LayoutParameters.Height != cardView.MeasuredHeight)
                {
                    viewPager.LayoutParameters.Height = cardView.MeasuredHeight;
                    viewPager.RequestLayout();
                }
            });
        }

        private class CarouselOnTouchListener : Java.Lang.Object, View.IOnTouchListener
        {
            public bool OnTouch(View sender, MotionEvent e)
            {
                if (e.Action != MotionEventActions.Move && e.Action != MotionEventActions.Up)
                    return false;
                
                var viewPager = (ViewPager2)sender.Parent;
                var view = viewPager?.FindViewWithTag("position-" + viewPager.CurrentItem);
                var cardView = view?.FindViewById<CardView>(Resource.Id.dashboardItem);
                
                int height;
                if (e.Action == MotionEventActions.Move)
                    height = (int)viewPager.Resources!.GetDimension(Resource.Dimension.carousel_current_item_max_height);
                else
                {
                    var wMeasureSpec = View.MeasureSpec.MakeMeasureSpec(cardView.Width, MeasureSpecMode.Exactly);
                    var hMeasureSpec = View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);
                    cardView.Measure(wMeasureSpec, hMeasureSpec);
                    height = cardView.MeasuredHeight;
                }
                
                cardView?.Post(() =>
                {
                    if (viewPager?.LayoutParameters != null && viewPager.LayoutParameters.Height != height)
                    {
                        viewPager.LayoutParameters.Height = height;
                        viewPager.RequestLayout();
                        view.RequestLayout();
                        cardView.RequestLayout();
                    }
                });

                return false;
            }
        }
    

        private class CarouselOnPageChangeCallback : ViewPager2.OnPageChangeCallback
        {
            private ViewPager2 viewPager;
            private int? prevPosition;

            public CarouselOnPageChangeCallback(ViewPager2 viewPager)
            {
                this.viewPager = viewPager;
            }

            public override void OnPageSelected(int position)
            {
                base.OnPageSelected(position);

                
                if (prevPosition.HasValue && prevPosition != position)
                {
                    var recyclerView = viewPager.GetChildAt(0) as RecyclerView;
                    var adapter = recyclerView?.GetAdapter() as MvxRecyclerAdapter;
                    var dashboardItem = adapter?.GetItem(prevPosition.Value) as IDashboardItem;
                    if (dashboardItem is { IsExpanded: true })
                        dashboardItem.IsExpanded = false;
                }

                prevPosition = position;
                
                
                var view = viewPager.FindViewWithTag("position-" + position);
                var cardView = view?.FindViewById<CardView>(Resource.Id.dashboardItem);
                
                cardView?.Post(() =>
                {
                    var wMeasureSpec = View.MeasureSpec.MakeMeasureSpec(cardView.Width, MeasureSpecMode.Exactly);
                    var hMeasureSpec = View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);
                    cardView.Measure(wMeasureSpec, hMeasureSpec);

                    if (viewPager?.LayoutParameters != null && viewPager.LayoutParameters.Height != cardView.MeasuredHeight)
                    {
                        viewPager.LayoutParameters.Height = cardView.MeasuredHeight;
                        viewPager.RequestLayout();
                        view.RequestLayout();
                        cardView.RequestLayout();
                    }
                });
            }
            
            protected override void Dispose(bool disposing)
            {
                viewPager = null;
                base.Dispose(disposing);
            }
        }

        protected virtual IMvxTemplateSelector CreateCarouselTemplateSelector() => new MapDashboardTemplateSelector();

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
            var viewPager = this.FindViewById<ViewPager2>(Resource.Id.carousel_view_pager);
            if (viewPager != null)
            {
                viewPager.LayoutChange -= ViewPagerOnLayoutChange;
            }
            
            onDrawerOpenedSubscription?.Dispose();
            onMapViewMapTappedSubscription?.Dispose();
            onPageChangeCallback?.Dispose();
            onTouchListener?.Dispose();

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
