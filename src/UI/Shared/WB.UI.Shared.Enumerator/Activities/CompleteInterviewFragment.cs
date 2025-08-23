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

        
        
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = base.OnCreateView(inflater, container, savedInstanceState);
            //var view = inflater.Inflate(Resource.Layout.interview_complete, container, false);

            tabLayout = view.FindViewById<TabLayout>(Resource.Id.tabLayout);
            viewPager = view.FindViewById<ViewPager2>(Resource.Id.viewPager);
            viewPager.OffscreenPageLimit = 2;
            viewPager.UserInputEnabled = false;

            SetupTabs();

            return view;
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
            //var adapter = new TabsPagerAdapter(this.Context, this.ChildFragmentManager, this.Lifecycle, tabsViewModels);
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
                    e.Tab.Select(); // prevent selection
                    viewPager.CurrentItem = viewPager.CurrentItem; // reset
                }

                UpdateTabViews();
                RecalculateRecyclerViewHeight();
            };

            // Optional: disable swipe
            viewPager.UserInputEnabled = true;

            var firstEnabledTab = tabsViewModels.FirstOrDefault(t => t.IsEnabled);
            if (firstEnabledTab != null)
            {
                var indexOf = tabsViewModels.IndexOf(firstEnabledTab);
                var tab = tabLayout.GetTabAt(indexOf);
                tab?.Select();
            }
            UpdateTabViews();
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

        private int MeasureItemHeight(MvxRecyclerView recyclerView, View view)
        {
            view.Measure(
                View.MeasureSpec.MakeMeasureSpec(recyclerView.Width, MeasureSpecMode.Exactly),
                View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified));
            var itemHeight = view.MeasuredHeight + view.PaddingTop + view.PaddingBottom;
            if (view.LayoutParameters is ViewGroup.MarginLayoutParams lp)
            {
                itemHeight += lp.TopMargin + lp.BottomMargin;
            }
            return itemHeight;
        }

        private int CalculateTotalHeight(MvxRecyclerView recyclerView)
        {
            int totalHeight = 0;
            var adapter = recyclerView.GetAdapter();

            if (adapter == null) return 0;

            for (int i = 0; i < adapter.ItemCount; i++)
            {
                int viewType = adapter.GetItemViewType(i);
                RecyclerView.ViewHolder viewHolder = (RecyclerView.ViewHolder)adapter.CreateViewHolder(recyclerView, viewType);
                adapter.BindViewHolder(viewHolder, i);
                totalHeight += MeasureItemHeight(recyclerView, viewHolder.ItemView);
            }

            totalHeight += recyclerView.PaddingTop + recyclerView.PaddingBottom;

            return totalHeight;
        }
        
        private void AdjustRecyclerViewHeight(object sender, NotifyCollectionChangedEventArgs e)
        {
            RecalculateRecyclerViewHeight();
        }

        private void RecalculateRecyclerViewHeight()
        {
            View?.PostDelayed(() =>
            {
                int currentItem = viewPager.CurrentItem; 
                var currentView = (viewPager.GetChildAt(0) as ViewGroup)?.GetChildAt(currentItem);
                var recyclerView = currentView?.FindViewById<MvxRecyclerView>(Resource.Id.recyclerView);

                if (recyclerView?.Visibility != ViewStates.Visible)
                    return;
                
                var layoutParams = recyclerView.LayoutParameters;
                var totalHeight = CalculateTotalHeight(recyclerView);
                layoutParams.Height = totalHeight;
                recyclerView.LayoutParameters = layoutParams;
                
                var moreLabel = currentView?.FindViewById<TextView>(Resource.Id.tab_content_more_label);
                if (moreLabel?.Visibility == ViewStates.Visible)
                {
                    moreLabel.Measure(
                        View.MeasureSpec.MakeMeasureSpec(View.Width, MeasureSpecMode.Exactly),
                        View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified));

                    totalHeight += moreLabel.MeasuredHeight + 36;
                }
                
                var viewPagerLayoutParams = viewPager.LayoutParameters;
                viewPagerLayoutParams.Height = totalHeight + 76;
                viewPager.LayoutParameters = viewPagerLayoutParams;
            }, 100);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
