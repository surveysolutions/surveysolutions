using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using CAPI.Android.Controls.QuestionnaireDetails;
using CAPI.Android.Core.Model.SnapshotStore;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using System.Linq;
using CAPI.Android.Events;
using CAPI.Android.Extensions;
using Cirrious.MvvmCross.Droid.Fragging;
using Microsoft.Practices.ServiceLocation;
using Ncqrs;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;


namespace CAPI.Android
{
    [Activity(NoHistory = true, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class DetailsActivity : MvxFragmentActivity, ViewTreeObserver.IOnGlobalLayoutListener, GestureDetector.IOnGestureListener
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
        //private readonly ILogger logger = ServiceLocator.Current.GetInstance<ILogger>();
        private GestureDetector gestureDetector;
        
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            if (this.FinishIfNotLoggedIn())
                return;

            this.Window.SetSoftInputMode(SoftInput.AdjustPan);
            
            ViewModel = CapiApplication.LoadView<QuestionnaireScreenInput, InterviewViewModel>(
                new QuestionnaireScreenInput(QuestionnaireId));

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
            gestureDetector = new GestureDetector(this);
            llNavigationHolder.Touch += btnNavigation_Touch;
            btnNavigation.Touch += btnNavigation_Touch;
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

        private void UpdateLayout(bool isNavigationVisible, bool animated = false)
        {
            AlignNavigationContainer(isNavigationVisible, animated);
            AlignScreenContainer(isNavigationVisible);
        }

        private void HidePanelAnimated()
        {
            isChaptersVisible = false;
            AlignNavigationContainer(false, true);
            AlignScreenContainer(false);
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

        private void AlignNavigationContainer(bool isNavigationVisible, bool animated)
        {
            var navigationContainerX = isNavigationVisible
                ? 0
                : btnNavigation.LayoutParameters.Width - lNavigationContainer.LayoutParameters.Width;
            if (animated)
                lNavigationContainer.Animate().TranslationX(navigationContainerX);
            else
                lNavigationContainer.SetX(navigationContainerX);
        }

        private void AlignBookmark()
        {
            llSpaceFiller.LayoutParameters.Height = (ScreenHeight - btnNavigation.LayoutParameters.Height) / 2;
        }

        void btnNavigation_Touch(object sender, View.TouchEventArgs e)
        {
            gestureDetector.OnTouchEvent(e.Event);
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

        private void ContentFrameAdapterScreenChanged(object sender, ScreenChangedEventArgs e)
        {
            var index = this.adapter.GetScreenIndex(e.ScreenId);

            if (sender == navList)
            {
                this.HidePanelAnimated();
            }

            if (index >= 0)
            {
                VpContent.CurrentItem = this.adapter.GetScreenIndex(e.ScreenId);

                adapter.NotifyDataSetChanged();
                return;
            }
            this.adapter.UpdateScreenData(e.ScreenId);
            adapter.NotifyDataSetChanged();

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

            if(btnNavigation != null)
                btnNavigation.Touch -= this.btnNavigation_Touch;
            if (llNavigationHolder != null)
                llNavigationHolder.Touch -= btnNavigation_Touch;
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

        public bool OnDown(MotionEvent e)
        {
            return true;
        }

        public bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            if (velocityX > 0 && !this.isChaptersVisible)
            {
                this.isChaptersVisible = true;
                this.UpdateLayout(true, true);
            }
            else if (velocityX < 0 && this.isChaptersVisible)
            {
                this.isChaptersVisible = false;
                this.UpdateLayout(false, true);
            }
            
            return true;
        }

        public void OnLongPress(MotionEvent e)
        {
        }

        public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
        {
            return true;
        }

        public void OnShowPress(MotionEvent e)
        {
        }

        public bool OnSingleTapUp(MotionEvent e)
        {
            this.isChaptersVisible = !this.isChaptersVisible;
            this.UpdateLayout(this.isChaptersVisible, true);
            return true;
        }
    }
}