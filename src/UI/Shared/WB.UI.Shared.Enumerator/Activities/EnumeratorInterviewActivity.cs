using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Plugins.Messenger;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.UI.Shared.Enumerator.CustomControls;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;
using System;
using System.Collections.Generic;
using Cirrious.MvvmCross.Droid.Views;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.CrossCore.Droid.Platform;

namespace WB.UI.Shared.Enumerator.Activities
{
    public interface IFragmentTypeLookup
    {
        bool TryGetFragmentType(Type viewModelType, out Type fragmentType);
    }

    public class FragmentTypeLookup : IFragmentTypeLookup
    {
        private readonly Dictionary<Type, Type> _fragmentLookup = new Dictionary<Type, Type>
        {
            { typeof(ActiveGroupViewModel), typeof(ActiveGroupFragment) },
            { typeof(CompleteInterviewViewModel), typeof(CompleteScreenFragment) }
        };

        public bool TryGetFragmentType(Type viewModelType, out Type fragmentType)
        {
            if (!_fragmentLookup.ContainsKey(viewModelType))
            {
                fragmentType = null;
                return false;
            }

            fragmentType = _fragmentLookup[viewModelType];
            return true;
        }
    }

    public class FragmentPresenter : MvxAndroidViewPresenter
    {
        private readonly IMvxViewModelLoader _viewModelLoader;
        private readonly IFragmentTypeLookup _fragmentTypeLookup;
        private FragmentManager _fragmentManager;

        public FragmentPresenter(IMvxViewModelLoader viewModelLoader, IFragmentTypeLookup fragmentTypeLookup)
        {
            _fragmentTypeLookup = fragmentTypeLookup;
            _viewModelLoader = viewModelLoader;
        }

        public void RegisterFragmentManager(FragmentManager fragmentManager, MvxFragment initialFragment)
        {
            _fragmentManager = fragmentManager;

            this.ShowFragment(initialFragment, false);
        }

        public override void Show(MvxViewModelRequest request)
        {
            Type fragmentType;
            if (_fragmentManager == null || !_fragmentTypeLookup.TryGetFragmentType(request.ViewModelType, out fragmentType))
            {
                base.Show(request);

                return;
            }

            var fragment = (MvxFragment)Activator.CreateInstance(fragmentType);
            fragment.ViewModel = _viewModelLoader.LoadViewModel(request, null);

            this.ShowFragment(fragment, true);
        }

        private void ShowFragment(MvxFragment fragment, bool addToBackStack)
        {
            var transaction = _fragmentManager.BeginTransaction();

            if (addToBackStack)
                transaction.AddToBackStack(fragment.GetType().Name);

            transaction
                .Replace(Resource.Id.contentFrame, fragment)
                .Commit();
        }

        public override void Close(IMvxViewModel viewModel)
        {
            var currentFragment = _fragmentManager.FindFragmentById(Resource.Id.contentFrame) as MvxFragment;
            if (currentFragment != null && currentFragment.ViewModel == viewModel)
            {
                _fragmentManager.PopBackStackImmediate();

                return;
            }

            base.Close(viewModel);
        }
    }

    public class ActiveGroupFragment : MvxFragment<ActiveGroupViewModel>
    {
        private MvxRecyclerView recyclerView;

        private LinearLayoutManager layoutManager;

        private InterviewEntityAdapter adapter;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var ignore = base.OnCreateView(inflater, container, savedInstanceState);

            var view = this.BindingInflate(Resource.Layout.interview_active_group, null);

            this.recyclerView = view.FindViewById<MvxRecyclerView>(Resource.Id.interviewEntitiesList);

            this.layoutManager = new LinearLayoutManager(Activity);
            this.recyclerView.SetLayoutManager(this.layoutManager);
            this.recyclerView.HasFixedSize = true;

            this.adapter = new InterviewEntityAdapter(Activity, (IMvxAndroidBindingContext)this.BindingContext);

            this.recyclerView.Adapter = this.adapter;
            return view;
        }
    }

    public class CompleteScreenFragment : MvxFragment<CompleteInterviewViewModel>
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var ignore = base.OnCreateView(inflater, container, savedInstanceState);

            var view = this.BindingInflate(Resource.Layout.interview_complete_screen, null);
            return view;
        }
    }

    public abstract class EnumeratorInterviewActivity<TViewModel> : BaseActivity<TViewModel> where TViewModel : EnumeratorInterviewViewModel
    {
        private ActionBarDrawerToggle drawerToggle;
        private DrawerLayout drawerLayout;
        private MvxSubscriptionToken sectionChangeSubscriptionToken;
        private MvxSubscriptionToken scrollToAnchorSubscriptionToken;

        private Toolbar toolbar;

        protected override int ViewResourceId
        {
            get { return Resource.Layout.interview; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            this.drawerLayout = this.FindViewById<DrawerLayout>(Resource.Id.drawer_layout);

            this.SetSupportActionBar(this.toolbar);
            this.SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            this.SupportActionBar.SetHomeButtonEnabled(true);

            this.drawerToggle = new ActionBarDrawerToggle(this, this.drawerLayout, this.toolbar, 0, 0);
            this.drawerLayout.SetDrawerListener(this.drawerToggle);
            this.drawerLayout.DrawerOpened += (sender, args) =>
            {
                this.RemoveFocusFromEditText();
                this.HideKeyboard(drawerLayout.WindowToken);
                var viewModel = this.ViewModel;
                viewModel.Sections.UpdateStatuses.Execute(null); // for some reason custom binding on drawerlayout is not working. 
            };

            var presenter = (FragmentPresenter)Mvx.Resolve<IMvxAndroidViewPresenter>();
            var initialFragment = Mvx.Resolve<ActiveGroupFragment>();
            initialFragment.ViewModel = ViewModel.CurrentGroup;
            presenter.RegisterFragmentManager(FragmentManager, initialFragment);
        }

        protected override void OnStart()
        {
            var messenger = Mvx.Resolve<IMvxMessenger>();
            this.sectionChangeSubscriptionToken = messenger.Subscribe<SectionChangeMessage>(this.OnSectionChange);
            this.scrollToAnchorSubscriptionToken = messenger.Subscribe<ScrollToAnchorMessage>(this.OnScrollToAnchorMessage);
            base.OnStart();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            this.Dispose();
        }

        private void OnSectionChange(SectionChangeMessage msg)
        {
            Application.SynchronizationContext.Post(_ => { this.drawerLayout.CloseDrawers();}, null);
        }

        private void OnScrollToAnchorMessage(ScrollToAnchorMessage msg)
        {
            //if (this.layoutManager != null)
            //{
            //    Application.SynchronizationContext.Post(_ =>
            //    {
            //        this.layoutManager.ScrollToPositionWithOffset(msg.AnchorElementIndex, 0);
            //    }, 
            //    null);
            //}
        }

        protected override void OnStop()
        {
            var messenger = Mvx.Resolve<IMvxMessenger>();
            messenger.Unsubscribe<SectionChangeMessage>(this.sectionChangeSubscriptionToken);
            messenger.Unsubscribe<ScrollToAnchorMessage>(this.scrollToAnchorSubscriptionToken);
            base.OnStop();
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            this.drawerToggle.SyncState();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(this.MenuResourceId, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (this.drawerToggle.OnOptionsItemSelected(item))
            {
                return true;
            }

            this.OnMenuItemSelected(item.ItemId);

            return base.OnOptionsItemSelected(item);
        }

        protected abstract int MenuResourceId { get; }
        protected abstract void OnMenuItemSelected(int resourceId);
    }
}