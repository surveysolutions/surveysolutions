﻿using Android.Views;
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
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Extensions.Activities.Carousel;
using WB.UI.Shared.Extensions.ViewModels;
using Math = Java.Lang.Math;
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

        public Toolbar Toolbar { get; private set; }

        private void Cancel()
        {
            this.Finish();
        }

        private static int carouselCurrentItemMaxHeight; 

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            carouselCurrentItemMaxHeight = (int)this.Resources.GetDimension(Resource.Dimension.carousel_current_item_max_height);
            
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

            ConfigureCarousel();

            this.ViewModel.MapView = this.FindViewById<MapView>(Resource.Id.map_view);
            onMapViewMapTappedSubscription = this.ViewModel.MapView.WeakSubscribe<MapView, GeoViewInputEventArgs>(
                nameof(this.ViewModel.MapView.GeoViewTapped),
                this.ViewModel.OnMapViewTapped);
        }

        private void ConfigureCarousel()
        {
            var viewPager = this.FindViewById<ViewPager2>(Resource.Id.carousel_view_pager);
            onPageChangeCallback = new CarouselOnPageChangeCallback(viewPager);
            viewPager.RegisterOnPageChangeCallback(onPageChangeCallback);
            viewPager.SystemUiVisibilityChange += ViewPagerOnSystemUiVisibilityChange;
            viewPager.ScrollChange += ViewPagerOnScrollChange;
            viewPager.LayoutChange += ViewPagerOnLayoutChange;
            viewPager.OffscreenPageLimit = 1;

            var recyclerView = (RecyclerView)viewPager.GetChildAt(0);
            recyclerView.Touch += RecyclerViewOnTouch;

            var adapter = new CarouselViewAdapter((IMvxAndroidBindingContext)base.BindingContext);
            adapter.ItemTemplateSelector = CreateCarouselTemplateSelector();
            adapter.ItemsSource = ViewModel.AvailableMarkers;
            viewPager.Adapter = adapter;

            var bindingSet = this.CreateBindingSet();
            bindingSet.Bind(adapter)
                .For(v => v.ItemsSource)
                .To(vm => vm.AvailableMarkers);
            bindingSet.Apply();

            var pageTransformer = new CarouselIPageTransformer(this);
            viewPager.SetPageTransformer(pageTransformer);
            var itemDecoration = new HorizontalMarginItemDecoration(
                this,
                Resource.Dimension.carousel_current_item_horizontal_margin
            );
            viewPager.AddItemDecoration(itemDecoration);
        }

        private void ViewPagerOnScrollChange(object sender, View.ScrollChangeEventArgs e)
        {
            var viewPager = (ViewPager2)sender;
            RecalculateCarouselHeight(viewPager);
        }

        private void ViewPagerOnSystemUiVisibilityChange(object sender, View.SystemUiVisibilityChangeEventArgs e)
        {
            if (e.Visibility == StatusBarVisibility.Hidden)
                return;
            
            var viewPager = (ViewPager2)sender;
            RecalculateCarouselHeight(viewPager);
        }

        private static bool isRecalculating = false;
        private static bool isRecalculatingAllowed = true;
        
        private static void RecalculateCarouselHeight(ViewPager2 viewPager)
        {
            if (!isRecalculatingAllowed || isRecalculating) return;
            isRecalculating = true;
            
            viewPager?.Post(() =>
                {
                    try
                    {
                        var view = viewPager.FindViewWithTag("position-" + viewPager.CurrentItem);
                        var cardView = view?.FindViewById<CardView>(Resource.Id.dashboardItem);

                        if (cardView == null) return;
                        
                        var wMeasureSpec = View.MeasureSpec.MakeMeasureSpec(cardView.Width, MeasureSpecMode.Exactly);
                        var hMeasureSpec = View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);
                        cardView.Measure(wMeasureSpec, hMeasureSpec);
                        var height = Math.Min(cardView.MeasuredHeight, carouselCurrentItemMaxHeight);

                        if (viewPager.LayoutParameters != null && viewPager.LayoutParameters.Height != height)
                        {
                            viewPager.LayoutParameters.Height = height;
                            viewPager.RequestLayout();
                        }
                        
                        view.RequestLayout();
                        
                        var scrollView = view.FindViewById<ScrollView>(Resource.Id.marker_card_scroll);
                        if (scrollView != null)
                        {
                            var layoutParams = (ConstraintLayout.LayoutParams) scrollView.LayoutParameters;
                            if (layoutParams != null)
                            {
                                layoutParams.MatchConstraintMaxHeight = height;
                                scrollView.LayoutParameters = layoutParams;
                            }
                            scrollView.RequestLayout();
                        }

                        cardView.RequestLayout();
                    }
                    finally
                    {
                        isRecalculating = false;
                    }
                });
        }

        private void ViewPagerOnLayoutChange(object sender, View.LayoutChangeEventArgs e)
        {
            var viewPager = (ViewPager2)sender;
            RecalculateCarouselHeight(viewPager);
        }
        
        private void RecyclerViewOnTouch(object sender, View.TouchEventArgs e)
        {
            e.Handled = false;
            
            if (e.Event == null)
                return;
            if (e.Event.Action != MotionEventActions.Move && e.Event.Action != MotionEventActions.Up)
                return;
                
            var viewPager = (ViewPager2)((View)sender).Parent;
                
            if (e.Event.Action == MotionEventActions.Move)
            {
                isRecalculatingAllowed = false;
                if (viewPager?.LayoutParameters != null && viewPager.LayoutParameters.Height < carouselCurrentItemMaxHeight)
                {
                    viewPager.LayoutParameters.Height = carouselCurrentItemMaxHeight;
                    viewPager.RequestLayout();
                }
            }
            else
            {
                isRecalculatingAllowed = true;
                RecalculateCarouselHeight(viewPager);
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

            public override void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
            {
                base.OnPageScrolled(position, positionOffset, positionOffsetPixels);
                
                if (prevPosition.HasValue && prevPosition != position)
                {
                    var recyclerView = viewPager.GetChildAt(0) as RecyclerView;
                    var adapter = recyclerView?.GetAdapter() as MvxRecyclerAdapter;
                    var dashboardItem = adapter?.GetItem(prevPosition.Value) as IDashboardItem;
                    if (dashboardItem is { IsExpanded: true })
                        dashboardItem.IsExpanded = false;
                }

                prevPosition = position;
                
                RecalculateCarouselHeight(viewPager);
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (drawerLayout != null)
                    drawerLayout.DrawerOpened -= OnDrawerLayoutOnDrawerOpened;

                var viewPager = this.FindViewById<ViewPager2>(Resource.Id.carousel_view_pager);
                if (viewPager != null)
                {
                    viewPager.UnregisterOnPageChangeCallback(onPageChangeCallback);
                    viewPager.SystemUiVisibilityChange -= ViewPagerOnSystemUiVisibilityChange;
                    viewPager.ScrollChange -= ViewPagerOnScrollChange;
                    viewPager.LayoutChange -= ViewPagerOnLayoutChange;

                    RecyclerView recyclerView = (RecyclerView) viewPager.GetChildAt(0);
                    recyclerView.Touch -= RecyclerViewOnTouch;
                }

                onDrawerOpenedSubscription?.Dispose();
                onMapViewMapTappedSubscription?.Dispose();
                onPageChangeCallback?.Dispose();
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
