using System;
using System.Linq;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Droid.Fragging;
using Ncqrs;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Capi.Shared.Adapters;
using WB.UI.Capi.Shared.Controls;
using WB.UI.Capi.Shared.Events;
using WB.UI.Capi.Shared.Frames;

namespace WB.UI.Capi.Shared.Activities
{
    public abstract class DetailsActivity : MvxFragmentActivity, ViewTreeObserver.IOnGlobalLayoutListener, GestureDetector.IOnGestureListener
    {
        protected Guid QuestionnaireId
        {
            get { return Guid.Parse(this.Intent.GetStringExtra("publicKey")); }
        }

        protected InterviewViewModel Model
        {
            get { return this.ViewModel as InterviewViewModel; }
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
            get { return this.llContainer.Width; }
        }

        protected int ScreenHeight
        {
            get { return this.llContainer.Height; }
        }

        private ContentFrameAdapter adapter;
        private QuestionnaireNavigationView navList;
        private bool isChaptersVisible = false;
        private InterviewItemId? screenId;
        private GestureDetector gestureDetector;
        
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.Window.SetSoftInputMode(SoftInput.AdjustPan);

            this.ViewModel = GetInterviewViewModel(this.QuestionnaireId);

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
                this.screenId = this.Model.Chapters.FirstOrDefault().ScreenId;
            }

            this.Title = this.Model.Title;
            this.ActionBar.SetDisplayShowHomeEnabled(false);

            if (bundle == null)
            {
                this.navList = new QuestionnaireNavigationView(this, this.Model);
                this.llNavigationHolder.AddView(this.navList);
                this.navList.ScreenChanged += this.ContentFrameAdapterScreenChanged;
            }
            else
            {
                this.navList = this.llNavigationHolder.GetChildAt(0) as QuestionnaireNavigationView;
            } 
            this.gestureDetector = new GestureDetector(this);
            this.llNavigationHolder.Touch += this.btnNavigation_Touch;
            this.btnNavigation.Touch += this.btnNavigation_Touch;
            this.adapter = CreateFrameAdapter(screenId);
            this.VpContent.Adapter = this.adapter;
            this.VpContent.PageSelected += this.VpContentPageSelected;
            this.llContainer.ViewTreeObserver.AddOnGlobalLayoutListener(this);
        }

        protected abstract ContentFrameAdapter CreateFrameAdapter(InterviewItemId? screenId);
        protected abstract InterviewViewModel GetInterviewViewModel(Guid interviewId);

        public void OnGlobalLayout()
        {
            this.llContainer.ViewTreeObserver.RemoveGlobalOnLayoutListener(this);

            this.AlignBookmark();
            this.ResizeNavigationPanel();
            this.UpdateLayout(this.isChaptersVisible);
        }

        private void ResizeNavigationPanel()
        {
            this.lNavigationContainer.LayoutParameters = new RelativeLayout.LayoutParams(this.ScreenWidth/2,
                this.lNavigationContainer.LayoutParameters.Height);
        }

        public override void OnConfigurationChanged(global::Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            this.isChaptersVisible = false;
            this.llContainer.ViewTreeObserver.AddOnGlobalLayoutListener(this);
        }

        private void UpdateLayout(bool isNavigationVisible, bool animated = false)
        {
            this.AlignNavigationContainer(isNavigationVisible, animated);
            this.AlignScreenContainer(isNavigationVisible);
        }

        private void HidePanelAnimated()
        {
            this.isChaptersVisible = false;
            this.AlignNavigationContainer(false, true);
            this.AlignScreenContainer(false);
        }

        private void AlignScreenContainer(bool isNavigationVisible)
        {
            var screenContainerWith = isNavigationVisible
                ? (this.ScreenWidth / 2 + this.btnNavigation.LayoutParameters.Width)
                : this.ScreenWidth;
            var screenContainerX = this.ScreenWidth - screenContainerWith;
            this.VpContent.LayoutParameters.Width = screenContainerWith;
            this.VpContent.SetX(screenContainerX);
            this.VpContent.RequestLayout();
        }

        private void AlignNavigationContainer(bool isNavigationVisible, bool animated)
        {
            var navigationContainerX = isNavigationVisible
                ? 0
                : this.btnNavigation.LayoutParameters.Width - this.lNavigationContainer.LayoutParameters.Width;
            if (animated)
                this.lNavigationContainer.Animate().TranslationX(navigationContainerX);
            else
                this.lNavigationContainer.SetX(navigationContainerX);
        }

        private void AlignBookmark()
        {
            this.llSpaceFiller.LayoutParameters.Height = (this.ScreenHeight - this.btnNavigation.LayoutParameters.Height) / 2;
        }

        void btnNavigation_Touch(object sender, View.TouchEventArgs e)
        {
            this.gestureDetector.OnTouchEvent(e.Event);
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

            if (sender == this.navList)
            {
                this.HidePanelAnimated();
            }

            if (index >= 0)
            {
                this.VpContent.CurrentItem = this.adapter.GetScreenIndex(e.ScreenId);

                this.adapter.NotifyDataSetChanged();
                return;
            }
            this.adapter.UpdateScreenData(e.ScreenId);
            this.adapter.NotifyDataSetChanged();

            this.VpContent.CurrentItem = this.adapter.GetScreenIndex(e.ScreenId);

            if (e.ScreenId.HasValue)
            {
                var screen = this.Model.Screens[e.ScreenId.Value];
                var chapterKey = screen.Breadcrumbs.First();
                for (int i = 0; i < this.Model.Chapters.Count; i++)
                {
                    if (this.Model.Chapters[i].ScreenId == chapterKey)
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

            if(this.btnNavigation != null)
                this.btnNavigation.Touch -= this.btnNavigation_Touch;
            if (this.llNavigationHolder != null)
                this.llNavigationHolder.Touch -= this.btnNavigation_Touch;
            if(this.VpContent != null)
                this.VpContent.PageSelected -= this.VpContentPageSelected;
            if(this.navList != null)
                this.navList.ScreenChanged -= this.ContentFrameAdapterScreenChanged;

            GC.Collect();
        }

        public override void OnLowMemory()
        {
            base.OnLowMemory();
            GC.Collect();
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