using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.View;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using CAPI.Android.Controls.QuestionnaireDetails;
using CAPI.Android.Core.Model;
using CAPI.Android.Core.Model.SnapshotStore;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using System.Linq;
using CAPI.Android.Events;
using CAPI.Android.Extensions;
using CAPI.Android.Services;
using Cirrious.MvvmCross.Droid.Fragging;
using Microsoft.Practices.ServiceLocation;
using Ncqrs;
using Ncqrs.Eventing.Storage;
using Ninject;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;


namespace CAPI.Android
{
    [Activity(NoHistory = true, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class DetailsActivity : MvxFragmentActivity, ViewTreeObserver.IOnGlobalLayoutListener
    {
        protected Guid QuestionnaireId
        {
            get { return Guid.Parse(Intent.GetStringExtra("publicKey")); }
        }

        protected InterviewViewModel Model
        {
            get { return ViewModel as InterviewViewModel; }
        }

        protected LinearLayout llSpaceFiller
        {
            get { return this.FindViewById<LinearLayout>(Resource.Id.llSpaceFiller); }
        }

        protected ViewPager VpContent
        {
            get { return this.FindViewById<ViewPager>(Resource.Id.vpContent); }
        }
        protected LinearLayout llNavigationHolder
        {
            get { return this.FindViewById<LinearLayout>(Resource.Id.llNavigationHolder); }
        }

        protected RelativeLayout lNavigationContainer
        {
            get { return this.FindViewById<RelativeLayout>(Resource.Id.lNavigationContainer); }
        }
        protected RelativeLayout llContainer
        {
            get { return this.FindViewById<RelativeLayout>(Resource.Id.llContainer); }
        }
        protected TextView btnNavigation
        {
            get { return this.FindViewById<TextView>(Resource.Id.btnNavigation); }
        }

        protected int ScreenWidth {
            get { return llContainer.Width; }
        }

        protected int ScreenHeight
        {
            get { return llContainer.Height; }
        }

        private ContentFrameAdapter adapter;
        private QuestionnaireNavigationView navList;
        private bool isChaptersVisible = false;
        private InterviewItemId? screenId;
        private readonly ILogger logger = ServiceLocator.Current.GetInstance<ILogger>();
        //private MoveNavigationPanelAnimation movePanelAnimation;

        protected override void OnCreate(Bundle bundle)
        {

            ViewModel = CapiApplication.LoadView<QuestionnaireScreenInput, InterviewViewModel>(
                new QuestionnaireScreenInput(QuestionnaireId));

            base.OnCreate(bundle);

            this.Window.SetSoftInputMode(SoftInput.AdjustPan);

            if (this.FinishIfNotLoggedIn())
                return;

            SetContentView(Resource.Layout.Details);


            if (bundle != null)
            {
                var savedScreen = bundle.GetString("ScreenId");
                if (!string.IsNullOrEmpty(savedScreen))
                {
                    this.screenId = InterviewItemId.Parse(savedScreen);
                }
            }
            else
            {
                this.screenId = Model.Chapters.FirstOrDefault().ScreenId;
            }

            this.Title = Model.Title;
            this.ActionBar.SetDisplayShowHomeEnabled(false);

            if (bundle == null)
            {
                this.navList = new QuestionnaireNavigationView(this, Model);
                llNavigationHolder.AddView(this.navList);
                this.navList.ScreenChanged += this.ContentFrameAdapterScreenChanged;
            }
            else
            {
                this.navList = llNavigationHolder.GetChildAt(0) as QuestionnaireNavigationView;
            }

            btnNavigation.Click += this.LlNavigationHolderClick;
            this.adapter = new ContentFrameAdapter(this.SupportFragmentManager, Model, this.screenId);
            VpContent.Adapter = this.adapter;
            VpContent.PageSelected += this.VpContentPageSelected;
            llContainer.ViewTreeObserver.AddOnGlobalLayoutListener(this);
        }

        public void OnGlobalLayout()
        {
            this.llContainer.ViewTreeObserver.RemoveGlobalOnLayoutListener(this);

            this.AlignBookmark();
            this.ResizeNavigationPanel();
            this.UpdateLayout(isChaptersVisible);
        }

        private void ResizeNavigationPanel()
        {
            this.lNavigationContainer.LayoutParameters = new RelativeLayout.LayoutParams(this.ScreenWidth/2,
                this.lNavigationContainer.LayoutParameters.Height);
        }

        public override void OnConfigurationChanged(global::Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            isChaptersVisible = false;
            llContainer.ViewTreeObserver.AddOnGlobalLayoutListener(this);
        }

        private void UpdateLayout(bool isNavigationVisible)
        {
            AlignScreenContainer(isNavigationVisible);

            AlignNavigationContainer(isNavigationVisible);
        }

        private void HidePanelAnimated()
        {
            isChaptersVisible = false;

            lNavigationContainer.Animate().TranslationX(btnNavigation.LayoutParameters.Width - lNavigationContainer.LayoutParameters.Width);

            AlignScreenContainer(false);

            adapter.NotifyDataSetChanged();
        }

        private void AlignScreenContainer(bool isNavigationVisible)
        {
            var screenContainerWith = isNavigationVisible
                ? (ScreenWidth / 2 + btnNavigation.LayoutParameters.Width)
                : ScreenWidth;
            var screenContainerX = ScreenWidth - screenContainerWith;
            VpContent.LayoutParameters.Width = screenContainerWith;
            VpContent.SetX(screenContainerX);
            VpContent.RequestLayout();
        }

        private void AlignNavigationContainer(bool isNavigationVisible)
        {
            var navigationContainerX = isNavigationVisible
                ? 0
                : btnNavigation.LayoutParameters.Width - lNavigationContainer.LayoutParameters.Width;

            lNavigationContainer.SetX(navigationContainerX);
        }

        private void AlignBookmark()
        {
            llSpaceFiller.LayoutParameters.Height = (ScreenHeight - btnNavigation.LayoutParameters.Height) / 2;
        }

        private void LlNavigationHolderClick(object sender, EventArgs e)
        {
            this.isChaptersVisible = !this.isChaptersVisible;

            this.UpdateLayout(isChaptersVisible);
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            if (!this.adapter.ScreenId.HasValue)
                return;
            outState.PutString("ScreenId", this.adapter.ScreenId.Value.ToString());
        }

        public override void OnAttachFragment(global::Android.Support.V4.App.Fragment p0)
        {
            var screen = p0 as IScreenChanging;
            if (screen != null)
            {
                screen.ScreenChanged += this.ContentFrameAdapterScreenChanged;
            }
            base.OnAttachFragment(p0);
        }

        void ContentFrameAdapterScreenChanged(object sender, ScreenChangedEventArgs e)
        {
            var index = this.adapter.GetScreenIndex(e.ScreenId);

            if (sender == navList)
            {
                this.HidePanelAnimated();
            }
            
            if (index >= 0)
            {
                VpContent.CurrentItem = this.adapter.GetScreenIndex(e.ScreenId);
                return;
            }

            this.adapter.UpdateScreenData(e.ScreenId);
            VpContent.CurrentItem = this.adapter.GetScreenIndex(e.ScreenId);

            if (e.ScreenId.HasValue)
            {
                var screen = Model.Screens[e.ScreenId.Value];
                var chapterKey = screen.Breadcrumbs.First();
                for (int i = 0; i < Model.Chapters.Count; i++)
                {
                    if (Model.Chapters[i].ScreenId == chapterKey)
                    {
                        this.navList.SelectItem(i);
                    }
                }
            }

            GC.Collect(0);
        }

        private void VpContentPageSelected(object sender, ViewPager.PageSelectedEventArgs e)
        {
            if (this.adapter.IsRoot)
                this.navList.SelectItem(e.Position);
            var statistic = this.adapter.GetItem(e.Position) as StatisticsContentFragment;
            if (statistic != null)
                statistic.RecalculateStatistics();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if(btnNavigation!= null)
                btnNavigation.Click -= this.LlNavigationHolderClick;
            if(VpContent != null)
                VpContent.PageSelected -= this.VpContentPageSelected;
            if(this.navList != null)
                this.navList.ScreenChanged -= this.ContentFrameAdapterScreenChanged;

            GC.Collect();
        }

        public override void OnLowMemory()
        {
            base.OnLowMemory();
            GC.Collect();
        }

        public override void Finish()
        {
            base.Finish();

            var snapshotStore = NcqrsEnvironment.Get<ISnapshotStore>() as AndroidSnapshotStore;
            if (snapshotStore != null)
                snapshotStore.PersistShapshot(QuestionnaireId);
        }
    }
}