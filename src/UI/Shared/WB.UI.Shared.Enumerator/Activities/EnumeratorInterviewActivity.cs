using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using MvvmCross.Platform;
using MvvmCross.Plugins.Messenger;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using DrawerLayout = Android.Support.V4.Widget.DrawerLayout;

namespace WB.UI.Shared.Enumerator.Activities
{
    public abstract class EnumeratorInterviewActivity<TViewModel> : BaseActivity<TViewModel>
        where TViewModel : EnumeratorInterviewViewModel
    {
        private ActionBarDrawerToggle drawerToggle;
        private DrawerLayout drawerLayout;
        private MvxSubscriptionToken sectionChangeSubscriptionToken;
        private MvxSubscriptionToken interviewCompleteActivityToken;
        private MvxSubscriptionToken countOfInvalidEntitiesIncreasedToken;
        private Vibrator vibrator;
        private Toolbar toolbar;

        protected override int ViewResourceId => Resource.Layout.interview;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            this.drawerLayout = this.FindViewById<DrawerLayout>(Resource.Id.drawer_layout);

            this.SetSupportActionBar(this.toolbar);
            this.SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            this.SupportActionBar.SetHomeButtonEnabled(true);

            this.drawerToggle = new ActionBarDrawerToggle(this, this.drawerLayout, this.toolbar, 0, 0);
            this.drawerLayout.AddDrawerListener(this.drawerToggle);
            this.drawerLayout.DrawerOpened += (sender, args) =>
            {
                this.RemoveFocusFromEditText();
                this.HideKeyboard(drawerLayout.WindowToken);
                var viewModel = this.ViewModel;
                viewModel.Sections.UpdateStatuses.Execute(null); // for some reason custom binding on drawerlayout is not working. 
            };
        }

        protected override void OnStart()
        {
            this.vibrator = (Vibrator)this.GetSystemService(Context.VibratorService);

            var messenger = Mvx.Resolve<IMvxMessenger>();

            this.sectionChangeSubscriptionToken = messenger.Subscribe<SectionChangeMessage>(this.OnSectionChange);
            this.interviewCompleteActivityToken = messenger.Subscribe<InterviewCompletedMessage>(this.OnInterviewCompleteActivity);
            this.countOfInvalidEntitiesIncreasedToken = messenger.Subscribe<CountOfInvalidEntitiesIncreasedMessage>(this.OnCountOfInvalidEntitiesIncreased);
            base.OnStart();
        }

        private void OnInterviewCompleteActivity(InterviewCompletedMessage obj)
        {
            this.Finish();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (IsFinishing)
            {
                this.Dispose();

                ViewModel.Dispose();
            }
        }

        private void OnCountOfInvalidEntitiesIncreased(CountOfInvalidEntitiesIncreasedMessage msg)
        {
            if (this.vibrator.HasVibrator)
                vibrator.Vibrate(100);
        }

        private void OnSectionChange(SectionChangeMessage msg)
        {
            Application.SynchronizationContext.Post(_ => { this.drawerLayout.CloseDrawers(); }, null);
        }

        protected override void OnStop()
        {
            var messenger = Mvx.Resolve<IMvxMessenger>();
            messenger.Unsubscribe<SectionChangeMessage>(this.sectionChangeSubscriptionToken);
            messenger.Unsubscribe<InterviewCompletedMessage>(this.interviewCompleteActivityToken);
            messenger.Unsubscribe<CountOfInvalidEntitiesIncreasedMessage>(this.countOfInvalidEntitiesIncreasedToken);
            base.OnStop();
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            this.drawerToggle.SyncState();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (this.drawerToggle.OnOptionsItemSelected(item))
            {
                return true;
            }

            this.OnMenuItemSelected(item);

            return base.OnOptionsItemSelected(item);
        }

        protected abstract void OnMenuItemSelected(IMenuItem item);

        protected void PopulateLanguagesMenu(IMenu menu, int languagesMenuId, int originalLanguageMenuItemId, int languagesMenuGroupId)
        {
            ISubMenu languagesMenu = menu.FindItem(languagesMenuId).SubMenu;

            IMenuItem currentLanguageMenuItem = menu.FindItem(originalLanguageMenuItemId);

            foreach (var language in this.ViewModel.AvailableTranslations)
            {
                var languageMenuItem = languagesMenu.Add(
                    groupId: languagesMenuGroupId,
                    itemId: Menu.None,
                    order: Menu.None,
                    title: language.Name);

                if (language.Id == this.ViewModel.CurrentLanguage)
                {
                    currentLanguageMenuItem = languageMenuItem;
                }
            }

            languagesMenu.SetGroupCheckable(languagesMenuGroupId, checkable: true, exclusive: true);

            currentLanguageMenuItem.SetChecked(true);
        }
    }
}