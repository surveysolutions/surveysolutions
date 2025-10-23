using System.Collections.Specialized;
using System.ComponentModel;
using Android.Content;
using Android.Content.Res;
using Android.Runtime;
using Android.Views;
using AndroidX.Core.Content;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.ViewPager2.Adapter;
using AndroidX.ViewPager2.Widget;
using Google.Android.Material.Tabs;
using MvvmCross;
using MvvmCross.Binding.BindingContext;
using MvvmCross.DroidX.RecyclerView;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Views.Fragments;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.UI.Shared.Enumerator.Converters;
using WB.UI.Shared.Enumerator.CustomControls;
using Fragment = Android.App.Fragment;

namespace WB.UI.Shared.Enumerator.Activities
{
    [Register("wb.ui.enumerator.activities.interview.CompleteInterviewFragment")]
    public class CompleteInterviewFragment : BaseFragment<CompleteInterviewViewModel>
    {
        protected override int ViewResourceId => Resource.Layout.interview_complete;

        private TabLayout tabLayout;
        private ViewPager2 viewPager;
        private ViewPager2.OnPageChangeCallback pageChangeCallback;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = base.OnCreateView(inflater, container, savedInstanceState);
            tabLayout = view.FindViewById<TabLayout>(Resource.Id.tabLayout);
            viewPager = view.FindViewById<ViewPager2>(Resource.Id.viewPager);
            viewPager.OffscreenPageLimit = 2;
            viewPager.UserInputEnabled = false;

            SetupTabs();
            RegisterPageChangeCallback();
            viewPager.Post(RecalculateRecyclerViewHeight);

            return view;
        }

        private void RegisterPageChangeCallback()
        {
            if (viewPager == null) return;
            pageChangeCallback = new HeightPageChangeCallback(this);
            viewPager.RegisterOnPageChangeCallback(pageChangeCallback);
        }

        private class HeightPageChangeCallback : ViewPager2.OnPageChangeCallback
        {
            private readonly CompleteInterviewFragment fragment;
            public HeightPageChangeCallback(CompleteInterviewFragment fragment) => this.fragment = fragment;
            public override void OnPageSelected(int position)
            {
                fragment.UpdateTabViews();
                fragment.viewPager?.Post(fragment.RecalculateRecyclerViewHeight);
            }
        }

        public class TabConfigurationStrategy2(CompleteInterviewViewModel viewModel)
            : Java.Lang.Object, TabLayoutMediator.ITabConfigurationStrategy
        {
            private CompleteInterviewViewModel viewModel = viewModel;

            public void OnConfigureTab(TabLayout.Tab tab, int position)
            {
                var tabVm = (TabViewModel)viewModel.Tabs[position];
                tab.SetText(tabVm.Title);

                if (!tabVm.IsEnabled)
                {
                    tab.View.Enabled = false;
                    tab.View.Alpha = 0.4f;
                }
            }

            protected override void Dispose(bool disposing)
            {
                this.viewModel = null;
                
                base.Dispose(disposing);
            }
        }
        
        public class TabConfigurationStrategy : Java.Lang.Object, TabLayoutMediator.ITabConfigurationStrategy
        {
            private readonly IMvxAndroidBindingContext bindingContext;
            private readonly IList<TabViewModel> tabs;

            public TabConfigurationStrategy(IMvxAndroidBindingContext bindingContext, IList<TabViewModel> tabs)
            {
                this.bindingContext = bindingContext;
                this.tabs = tabs;
            }

            public void OnConfigureTab(TabLayout.Tab tab, int position)
            {
                var vm = tabs[position];
                
                //var inflater = Mvx.IoCProvider.Resolve<LayoutInflater>();

                //var inflater = LayoutInflater.From(tab.View.Context);
                //var view = inflater.Inflate(Resource.Layout.interview_complete_tab_item, null);
                //var view = this.BindingInflate(Resource.Layout.interview_complete_tab_item, null);
                var view = bindingContext.BindingInflate(Resource.Layout.interview_complete_tab_item, null);

                var countView = view.FindViewById<TextView>(Resource.Id.tab_count);
                var titleView = view.FindViewById<TextView>(Resource.Id.tab_title);
                var indicator = view.FindViewById<View>(Resource.Id.tab_indicator);

                countView.Text = vm.Count; 
                titleView.Text = vm.Title; 
                tab.View.Enabled = vm.IsEnabled;

                var colorResInt = TabContentToColorConverter.GetColor(vm);
                var colorInt = ContextCompat.GetColor(tab.View.Context, colorResInt);
                var color = new Android.Graphics.Color(colorInt);
                countView?.SetTextColor(color);
                titleView?.SetTextColor(color);
                indicator.BackgroundTintList = ColorStateList.ValueOf(color);

                tab.SetCustomView(view);
                
                //bindingContext.DataContext = vm;

                //view.DataContext = vm;
                //view.ViewModel = vm;
            }
        }

        private void SetupTabs()
        {
            var viewModel = ViewModel;
            var tabsViewModels = viewModel.Tabs.ToList();
            var adapter = new TabsPagerAdapter(this.Context, this.ChildFragmentManager, this.Lifecycle, tabsViewModels);
            viewPager.Adapter = adapter;

            var tabConfigurationStrategy = new TabConfigurationStrategy((IMvxAndroidBindingContext)this.BindingContext, tabsViewModels);
            var tabLayoutMediator = new TabLayoutMediator(tabLayout, viewPager, tabConfigurationStrategy);
            tabLayoutMediator.Attach();

            tabLayout.TabSelected += (s, e) =>
            {
                var position = e.Tab.Position;
                if (!((TabViewModel)viewModel.Tabs[position]).IsEnabled)
                {
                    viewPager.Post(() => viewPager.SetCurrentItem(viewPager.CurrentItem, false));
                    return;
                }
                UpdateTabViews();
            };
            
            int firstNonEmptyIndex = tabsViewModels.FindIndex(t => t.IsEnabled);
            if (firstNonEmptyIndex > 0)
            {
                viewPager.Post(() =>
                {
                    if (firstNonEmptyIndex < viewModel.Tabs.Count)
                    {
                        viewPager.SetCurrentItem(firstNonEmptyIndex, false);
                        UpdateTabViews();
                        RecalculateRecyclerViewHeight();
                    }
                });
            }
            else
            {
                UpdateTabViews();
            }
        }
        
        private View CreateTabView(string title, string count, bool isSelected)
        {
            var inflater = LayoutInflater.From(this.Context);
            var view = inflater.Inflate(Resource.Layout.interview_complete_tab_item, null);

            var countView = view.FindViewById<TextView>(Resource.Id.tab_count);
            var titleView = view.FindViewById<TextView>(Resource.Id.tab_title);

            countView.Text = count;
            titleView.Text = title;

            if (isSelected)
            {
                //countView.SetTextColor(ContextCompat.GetColor(this.Context, Resource.Color.material_blue_grey_800));
                //titleView.SetTextColor(ContextCompat.GetColor(this.Context, Resource.Color.material_blue_grey_800));
            }

            return view;
        }

        private void UpdateTabViews()
        {
            for (int i = 0; i < tabLayout.TabCount; i++)
            {
                var tab = tabLayout.GetTabAt(i);
                var isSelected = tab?.IsSelected ?? false;
                var view = tab?.CustomView;

                if (view == null) continue;

                var countView = view.FindViewById<TextView>(Resource.Id.tab_count);
                var titleView = view.FindViewById<TextView>(Resource.Id.tab_title);
                var indicator = view.FindViewById<View>(Resource.Id.tab_indicator);

                // var colorResInt = TabContentToColorConverter.GetColor((TabViewModel)ViewModel.Tabs[i]);
                // var colorInt = ContextCompat.GetColor(Context, colorResInt);
                // var color = new Android.Graphics.Color(colorInt);
                // countView?.SetTextColor(color);
                // titleView?.SetTextColor(color);
                // indicator?.SetBackgroundColor(color);

                // var colorInt = ContextCompat.GetColor(Context, isSelected ? Resource.Color.material_blue_grey_800 : Resource.Color.disabledTextColor);
                // var color =  new Android.Graphics.Color(colorInt);
                // countView?.SetTextColor(color);
                // titleView?.SetTextColor(color);
                indicator.Visibility = isSelected ? ViewStates.Visible : ViewStates.Invisible;

                view.Selected = isSelected;
            }
        }
        
        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            
            // recyclerView = view.FindViewById<MvxRecyclerView>(Resource.Id.tv_Complete_Groups);
            // recyclerView.SetLayoutManager(new MvxGuardedLinearLayoutManager(Context));
            // recyclerView.SetItemAnimator(null);
            
            //ViewModel.CompleteGroups.CollectionChanged += AdjustRecyclerViewHeight;
        }

        private int CalculateTotalHeight(MvxRecyclerView recyclerView)
        {
            int totalHeight = 0;
            var adapter = recyclerView.GetAdapter();
            if (adapter == null) return 0;

            int width = recyclerView.MeasuredWidth > 0 ? recyclerView.MeasuredWidth : recyclerView.Width;
            if (width <= 0) return 0;
            int widthSpec = View.MeasureSpec.MakeMeasureSpec(width, MeasureSpecMode.Exactly);

            for (int i = 0; i < adapter.ItemCount; i++)
            {
                int viewType = adapter.GetItemViewType(i);
                var viewHolder = (RecyclerView.ViewHolder)adapter.CreateViewHolder(recyclerView, viewType);
                adapter.BindViewHolder(viewHolder, i);
                var itemView = viewHolder.ItemView;
                itemView.Measure(widthSpec, View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified));
                int h = itemView.MeasuredHeight;
                if (itemView.LayoutParameters is ViewGroup.MarginLayoutParams mlp)
                    h += mlp.TopMargin + mlp.BottomMargin;
                totalHeight += h;
            }

            totalHeight += recyclerView.PaddingTop + recyclerView.PaddingBottom;
            if (recyclerView.LayoutParameters is ViewGroup.MarginLayoutParams lp)
                totalHeight += lp.TopMargin + lp.BottomMargin;

            return totalHeight;
        }

        private int GetRecyclerContentHeight(MvxRecyclerView recyclerView)
        {
            int scrollRange = recyclerView.ComputeVerticalScrollRange();
            if (scrollRange > 0)
            {
                return scrollRange; 
            }
            return CalculateTotalHeight(recyclerView);
        }

        private int GetVisibleRecyclerHeight(MvxRecyclerView recyclerView)
        {
            int childCount = recyclerView.ChildCount;
            if (childCount == 0) return 0;
            int minTop = int.MaxValue;
            int maxBottom = 0;
            for (int i = 0; i < childCount; i++)
            {
                var child = recyclerView.GetChildAt(i);
                if (child == null) continue;
                int top = child.Top;
                int bottom = child.Bottom;
                if (top < minTop) minTop = top;
                if (bottom > maxBottom) maxBottom = bottom;
            }
            if (minTop == int.MaxValue) return 0;
            int visibleHeight = (maxBottom - minTop) + recyclerView.PaddingTop + recyclerView.PaddingBottom;
            if (recyclerView.LayoutParameters is ViewGroup.MarginLayoutParams lp)
                visibleHeight += lp.TopMargin + lp.BottomMargin;
            return visibleHeight;
        }

        private void RecalculateRecyclerViewHeight()
        {
            viewPager?.Post(() =>
            {
                if (viewPager == null) return;
                int currentItem = viewPager.CurrentItem;
                var currentViewGroup = viewPager.GetChildAt(0) as ViewGroup; 
                var currentView = currentViewGroup?.GetChildAt(currentItem);
                var recyclerView = currentView?.FindViewById<MvxRecyclerView>(Resource.Id.recyclerView);
                if (recyclerView == null || recyclerView.Visibility != ViewStates.Visible)
                {
                    viewPager.PostDelayed(RecalculateRecyclerViewHeight, 50);
                    return;
                }

                if (recyclerView.Width == 0 && recyclerView.MeasuredWidth == 0)
                {
                    recyclerView.Post(RecalculateRecyclerViewHeight);
                    return;
                }

                int contentHeight = GetRecyclerContentHeight(recyclerView);
                if (contentHeight == 0)
                {
                    recyclerView.PostDelayed(RecalculateRecyclerViewHeight, 50);
                    return;
                }
                
                var moreLabel = currentView?.FindViewById<TextView>(Resource.Id.tab_content_more_label);
                int extraHeight = 0;
                if (moreLabel?.Visibility == ViewStates.Visible)
                    extraHeight += GetViewHeight(moreLabel, viewPager);
                
                var rvParams = recyclerView.LayoutParameters;
                rvParams.Height = contentHeight; 
                rvParams.Width = ViewGroup.LayoutParams.MatchParent;
                recyclerView.LayoutParameters = rvParams;

                var vpParams = viewPager.LayoutParameters;
                vpParams.Height = contentHeight + extraHeight + 90; 
                vpParams.Width = ViewGroup.LayoutParams.MatchParent;
                viewPager.LayoutParameters = vpParams;

                viewPager.RequestLayout();
            });
        }
        
        private int GetViewHeight(View view, View parent)
        {
            int parentWidth = parent.MeasuredWidth > 0 ? parent.MeasuredWidth : parent.Width;
            if (parentWidth <= 0)
                parentWidth = ViewGroup.LayoutParams.MatchParent;

            view.Measure(
                View.MeasureSpec.MakeMeasureSpec(parentWidth, MeasureSpecMode.Exactly),
                View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified));

            int itemHeight = view.MeasuredHeight;
            if (view.LayoutParameters is ViewGroup.MarginLayoutParams lp)
                itemHeight += lp.TopMargin + lp.BottomMargin;
            return itemHeight;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && viewPager != null && pageChangeCallback != null)
            {
                viewPager.UnregisterOnPageChangeCallback(pageChangeCallback);
                pageChangeCallback = null;
            }
            base.Dispose(disposing);
        }
    }
}
