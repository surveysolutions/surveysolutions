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

        private ContentFrameAdapter adapter;
        private QuestionnaireNavigationView navList;
        private bool isChaptersVisible = false;
        private InterviewItemId? screenId;
        private readonly ILogger logger = ServiceLocator.Current.GetInstance<ILogger>();

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
            llContainer.ViewTreeObserver.RemoveGlobalOnLayoutListener(this);
            UpdateLayout(isChaptersVisible);
        }

        public override void OnConfigurationChanged(global::Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            isChaptersVisible = false;
            llContainer.ViewTreeObserver.AddOnGlobalLayoutListener(this);
        }

        private void UpdateLayout(bool isNavigationVisible)
        {
            var point = GetScreenSizePoint();
            var vpContentParams =
                new RelativeLayout.LayoutParams(
                    isNavigationVisible
                        ? (point.X / 2 + btnNavigation.LayoutParameters.Width)
                        : point.X,
                    ViewGroup.LayoutParams.FillParent);

            vpContentParams.LeftMargin = point.X - vpContentParams.Width;
            if(VpContent == null)
                logger.Error("Container was destroyed. Null ref will be thrown.");

            VpContent.LayoutParameters = vpContentParams;

            var lNavigationContainerParams =
               new RelativeLayout.LayoutParams(point.X / 2,
                                               ViewGroup.LayoutParams.FillParent);

            lNavigationContainerParams.LeftMargin = isNavigationVisible
                                                        ? 0 : btnNavigation.LayoutParameters.Width - lNavigationContainerParams.Width;

            lNavigationContainer.LayoutParameters = lNavigationContainerParams;
            this.navList.Visibility = isNavigationVisible ? ViewStates.Visible : ViewStates.Gone;

            llSpaceFiller.LayoutParameters = new LinearLayout.LayoutParams(llSpaceFiller.LayoutParameters.Width, (point.Y - btnNavigation.LayoutParameters.Height) / 2);
        }

        private Point GetScreenSizePoint()
        {
            return new Point(llContainer.Width,llContainer.Height);
        }

        private void LlNavigationHolderClick(object sender, EventArgs e)
        {
            var point = GetScreenSizePoint();
            int right, left;
            if (isChaptersVisible)
            {
                right = btnNavigation.LayoutParameters.Width;
                left = btnNavigation.LayoutParameters.Width - point.X / 2;

                isChaptersVisible = false;
            }
            else
            {
                right = point.X / 2;
                left = 0;

                isChaptersVisible = true;
            }

            UpdateLayout(isChaptersVisible);

            lNavigationContainer.Layout(left, lNavigationContainer.Top, right, lNavigationContainer.Bottom);

            VpContent.Layout(right, VpContent.Top, right + VpContent.Width, VpContent.Bottom);

            VpContent.RequestLayout();
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

            if (index >= 0)
            {
                if (sender == navList)
                {
                    Task.Factory.StartNew(() =>
                    {
                        Thread.Sleep(1000);
                        isChaptersVisible = false;
                        this.RunOnUiThread(() => this.UpdateLayout(false));
                    });
                }

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