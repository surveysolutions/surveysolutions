using System.Collections.Specialized;
using System.ComponentModel;
using Android.Runtime;
using Android.Views;
using AndroidX.Core.Content;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.ViewPager2.Adapter;
using AndroidX.ViewPager2.Widget;
using Google.Android.Material.Tabs;
using MvvmCross.DroidX.RecyclerView;
using MvvmCross.Platforms.Android.Views.Fragments;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.UI.Shared.Enumerator.CustomControls;
using Fragment = Android.App.Fragment;

namespace WB.UI.Shared.Enumerator.Activities
{
    [Register("wb.ui.enumerator.activities.interview.CompleteInterviewFragment")]
    public class CompleteInterviewFragment : BaseFragment<CompleteInterviewViewModel>
    {
        protected override int ViewResourceId => Resource.Layout.interview_complete;

        private MvxRecyclerView recyclerView;
        private TabLayout tabLayout;
        private ViewPager2 viewPager;

        
        
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = base.OnCreateView(inflater, container, savedInstanceState);
            //var view = inflater.Inflate(Resource.Layout.interview_complete, container, false);

            tabLayout = view.FindViewById<TabLayout>(Resource.Id.tabLayout);
            viewPager = view.FindViewById<ViewPager2>(Resource.Id.viewPager);

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
            private readonly IList<TabViewModel> tabs;

            public TabConfigurationStrategy(IList<TabViewModel> tabs)
            {
                this.tabs = tabs;
            }

            public void OnConfigureTab(TabLayout.Tab tab, int position)
            {
                var vm = tabs[position];

                var view = LayoutInflater.From(tab.View.Context).Inflate(Resource.Layout.tab_item, null);
                var countView = view.FindViewById<TextView>(Resource.Id.tab_count);
                var titleView = view.FindViewById<TextView>(Resource.Id.tab_title);
                var indicator = view.FindViewById<View>(Resource.Id.tab_indicator);

                countView.Text = vm.Count; 
                titleView.Text = vm.Title; 
                tab.View.Enabled = vm.IsEnabled;

                tab.SetCustomView(view);
            }
        }

        private void SetupTabs()
        {
            var viewModel = ViewModel;
            var tabsViewModels = viewModel.Tabs.ToList();
            //var adapter = new TabsPagerAdapter(this.Context, this.ChildFragmentManager, this.Lifecycle, tabsViewModels);
            var adapter = new TabsPagerAdapter(this.Context, this.ChildFragmentManager, this.Lifecycle, tabsViewModels);
            viewPager.Adapter = adapter;

            var tabLayoutMediator = new TabLayoutMediator(tabLayout, viewPager, new TabConfigurationStrategy(tabsViewModels));
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
            var view = inflater.Inflate(Resource.Layout.tab_item, null);

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

                var colorInt = ContextCompat.GetColor(Context, isSelected ? Resource.Color.material_blue_grey_800 : Resource.Color.disabledTextColor);
                var color =  new Android.Graphics.Color(colorInt);
                countView?.SetTextColor(color);
                titleView?.SetTextColor(color);
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

        private int MeasureItemHeight(View view)
        {
            view.Measure(
                View.MeasureSpec.MakeMeasureSpec(recyclerView.Width, MeasureSpecMode.Exactly),
                View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified));
            return view.MeasuredHeight;
        }

        private int CalculateTotalHeight()
        {
            int totalHeight = 0;
            var adapter = recyclerView.GetAdapter();

            if (adapter == null) return 0;

            for (int i = 0; i < adapter.ItemCount; i++)
            {
                int viewType = adapter.GetItemViewType(i);
                RecyclerView.ViewHolder viewHolder = (RecyclerView.ViewHolder)adapter.CreateViewHolder(recyclerView, viewType);
                adapter.BindViewHolder(viewHolder, i);
                totalHeight += MeasureItemHeight(viewHolder.ItemView);
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
            recyclerView = viewPager?.FindViewById<MvxRecyclerView>(Resource.Id.recyclerView);
            recyclerView?.SetLayoutManager(new MvxGuardedLinearLayoutManager(Context));
            
            if (recyclerView?.Visibility != ViewStates.Visible)
                return;
            
            recyclerView.Post(() =>
            {
                var layoutParams = recyclerView.LayoutParameters;
                layoutParams.Height = CalculateTotalHeight();
                recyclerView.LayoutParameters = layoutParams;
            });
        }

        protected override void Dispose(bool disposing)
        {
            if(ViewModel != null && ViewModel.CompleteGroups != null)
                ViewModel.CompleteGroups.CollectionChanged -= AdjustRecyclerViewHeight;
            
            base.Dispose(disposing);
        }
    }
}
