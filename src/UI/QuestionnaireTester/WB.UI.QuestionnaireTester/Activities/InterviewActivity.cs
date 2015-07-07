using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Plugins.Messenger;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.QuestionnaireTester.CustomControls;

namespace WB.UI.QuestionnaireTester.Activities
{
    [Activity(Label = "", Theme = "@style/BlueAppTheme", HardwareAccelerated = true,
        WindowSoftInputMode = SoftInput.StateAlwaysHidden | SoftInput.AdjustPan,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class InterviewActivity : BaseActivity<InterviewViewModel>
    {
        private ActionBarDrawerToggle drawerToggle;
        private DrawerLayout drawerLayout;
        private MvxSubscriptionToken sectionChangeSubscriptionToken;
        private MvxSubscriptionToken scrollToAnchorSubscriptionToken;

        private LinearLayoutManager layoutManager;

        private Toolbar toolbar;

        private MvxRecyclerView recyclerView;

        protected override int ViewResourceId
        {
            get { return Resource.Layout.interview; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            drawerLayout = this.FindViewById<DrawerLayout>(Resource.Id.drawer_layout);

            this.recyclerView = this.FindViewById<MvxRecyclerView>(Resource.Id.questionnaireEntitiesList);

            this.SetSupportActionBar(this.toolbar);
            this.SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            this.SupportActionBar.SetHomeButtonEnabled(true);

            this.drawerToggle = new ActionBarDrawerToggle(this, drawerLayout, this.toolbar, 0, 0);
            drawerLayout.SetDrawerListener(this.drawerToggle);
            drawerLayout.DrawerOpened += (sender, args) =>
            {
                this.RemoveFocusFromEditText();
                this.HideKeyboard(drawerLayout.WindowToken);
            };

            this.layoutManager = new LinearLayoutManager(this);
            this.recyclerView.SetLayoutManager(this.layoutManager);
            this.recyclerView.HasFixedSize = true;
            this.recyclerView.Adapter = new InterviewEntityAdapter(this, (IMvxAndroidBindingContext)this.BindingContext);

            var messenger = Mvx.Resolve<IMvxMessenger>();

            sectionChangeSubscriptionToken = messenger.Subscribe<SectionChangeMessage>(this.OnSectionChange);
            scrollToAnchorSubscriptionToken = messenger.Subscribe<ScrollToAnchorMessage>(this.OnScrollToAnchorMessage);
        }

        private void OnSectionChange(SectionChangeMessage msg)
        {
            drawerLayout.CloseDrawers();
        }

        private void OnScrollToAnchorMessage(ScrollToAnchorMessage msg)
        {
            if (this.layoutManager != null)
            {
                // recyclerView's adapter contains new view models, but layoutManager contains views from previous screen.
                this.layoutManager.ScrollToPositionWithOffset(msg.AnchorElementIndex, 0);
                if (msg.OffsetInsideOfAnchoredItemInPercentage != 0)
                {
                    // we scrolled to AnchorElementIndex, so it might be the first in list of elements on screen
                    View anchoredItemView = this.layoutManager.GetChildAt(0);

                    //// tried this one too
                    //var visibleItemPosition = this.layoutManager.FindFirstCompletelyVisibleItemPosition();
                    //View anchoredItemView = this.layoutManager.GetChildAt(visibleItemPosition);

                    // If you'll try this questionnaire
                    // https://design-devalt.mysurvey.solutions/UpdatedDesigner/app/#/0d92c64c5cbb4cd4b8c9dd8d6fe73614
                    // you'll always get null here while going back and forth with 'Section 1' and 'Numeric roster' items
                    // try Section 2 for testing, because it contains elements with different heights and its easier to test.
                    if (anchoredItemView != null)
                    {
                        // OffsetInsideOfAnchoredItemInPercentage is from 0 to 100
                        int offset = anchoredItemView.Height * msg.OffsetInsideOfAnchoredItemInPercentage / 100;
                        // negative offset scrolls down.
                        // here we trying to scroll elements on current screen based on measurements from previous screen
                        this.layoutManager.ScrollToPositionWithOffset(msg.AnchorElementIndex, -offset);
                    }
                }
            }
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            this.drawerToggle.SyncState();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.interview, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (this.drawerToggle.OnOptionsItemSelected(item))
            {
                return true;
            }

            switch (item.ItemId)
            {
                case Resource.Id.interview_dashboard:
                    this.ViewModel.NavigateToDashboardCommand.Execute();
                    break;
                case Resource.Id.interview_settings:
                    Intent intent = new Intent(this, typeof(PrefsActivity));
                    this.StartActivity(intent);
                    break;
                case Resource.Id.interview_signout:
                    this.ViewModel.SignOutCommand.Execute();
                    break;

            }
            return base.OnOptionsItemSelected(item);
        }
    }
}