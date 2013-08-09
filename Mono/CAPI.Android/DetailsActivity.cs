using System;
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
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using System.Linq;
using CAPI.Android.Events;
using CAPI.Android.Extensions;
using CAPI.Android.Services;
using Cirrious.MvvmCross.Droid.Fragging;
using Ninject;


namespace CAPI.Android
{
    [Activity(NoHistory = true, Icon = "@drawable/capi", ConfigurationChanges = ConfigChanges.Orientation |ConfigChanges.KeyboardHidden |ConfigChanges.ScreenSize)]
    public class DetailsActivity : MvxFragmentActivity/*<CompleteQuestionnaireView>, View.IOnTouchListener*/
    {
        protected ItemPublicKey? ScreenId;
        protected FrameLayout FlDetails
        {
            get { return this.FindViewById<FrameLayout>(Resource.Id.flDetails); }
        }
        protected Guid QuestionnaireId
        {
            get { return Guid.Parse(Intent.GetStringExtra("publicKey")); }
        }

        protected CompleteQuestionnaireView Model {
            get { return ViewModel as CompleteQuestionnaireView; }
        }

        protected ViewPager VpContent
        {
            get { return this.FindViewById<ViewPager>(Resource.Id.vpContent); }
        }
        protected LinearLayout llNavigationHolder
        {
            get { return this.FindViewById<LinearLayout>(Resource.Id.llNavigationHolder); }
        }

        protected FrameLayout flSpaceFiller
        {
            get { return this.FindViewById<FrameLayout>(Resource.Id.flSpaceFiller); }
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
        protected ContentFrameAdapter Adapter { get; set; }
        protected QuestionnaireNavigationFragment NavList { get; set; }

        protected CleanUpExecutor cleanUpExecutor { get; set; }

        protected override void OnCreate(Bundle bundle)
        {

            ViewModel = CapiApplication.LoadView<QuestionnaireScreenInput, CompleteQuestionnaireView>(
                new QuestionnaireScreenInput(QuestionnaireId));

            base.OnCreate(bundle);

            if (this.FinishIfNotLoggedIn())
                return;
            SetContentView(Resource.Layout.Details);


            if (bundle != null)
            {
                var savedScreen = bundle.GetString("ScreenId");
                if (!string.IsNullOrEmpty(savedScreen))
                {
                    ScreenId = ItemPublicKey.Parse(savedScreen);
                }
            }
            else
            {
                ScreenId = Model.Chapters.FirstOrDefault().ScreenId;
            }

            this.Title = Model.Title;

            if (bundle == null)
            {
                NavList = QuestionnaireNavigationFragment.NewInstance(Model.PublicKey);
                this.SupportFragmentManager.BeginTransaction()
                    .Add(Resource.Id.llNavigationHolder, NavList, "navigation")
                    .Commit();
            }
            else
            {
                NavList = this.SupportFragmentManager.FindFragmentByTag("navigation") as QuestionnaireNavigationFragment;
            }

            btnNavigation.Click += llNavigationHolder_Click;
            Adapter = new ContentFrameAdapter(this.SupportFragmentManager, Model, ScreenId);
            VpContent.Adapter = Adapter;
            VpContent.PageSelected += VpContent_PageSelected;

            cleanUpExecutor = new CleanUpExecutor(CapiApplication.Kernel.Get<IChangeLogManipulator>());

        }

        protected override void OnResume()
        {
            base.OnResume();
            UpdateLayout(false);
        }
        public override void OnConfigurationChanged(global::Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            isChaptersVisible = false;
            UpdateLayout(false);
        }

        private void UpdateLayout(bool isNavigationVisible)
        {

            var  point = GetScreenSizePoint();
            var vpContentParams =
                new RelativeLayout.LayoutParams(
                    isNavigationVisible
                        ? (point.X / 2 + btnNavigation.LayoutParameters.Width)
                        : point.X,
                    ViewGroup.LayoutParams.FillParent);

            vpContentParams.LeftMargin = point.X - vpContentParams.Width;
            VpContent.LayoutParameters = vpContentParams;

            var lNavigationContainerParams =
               new RelativeLayout.LayoutParams(point.X / 2,
                                               ViewGroup.LayoutParams.FillParent);

            lNavigationContainerParams.LeftMargin = isNavigationVisible
                                                        ? 0 : btnNavigation.LayoutParameters.Width - lNavigationContainerParams.Width;

            lNavigationContainer.LayoutParameters = lNavigationContainerParams;

            flSpaceFiller.LayoutParameters = new LinearLayout.LayoutParams(40, point.Y - 100);
        }

        protected Point GetScreenSizePoint()
        {
            var rectSize = new Rect();
            this.WindowManager.DefaultDisplay.GetRectSize(rectSize);

            TypedValue tv = new TypedValue();
            int actionBarHeight = 0;

            if (this.Theme.ResolveAttribute(global::Android.Resource.Attribute.ActionBarSize, tv, true))
            {
                actionBarHeight = TypedValue.ComplexToDimensionPixelSize(tv.Data, this.Resources.DisplayMetrics);
                
            }
            return new Point(rectSize.Width(), rectSize.Height() - actionBarHeight);
        }

        private bool isChaptersVisible = false;

        private void llNavigationHolder_Click(object sender, EventArgs e)
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
        
            if (!Adapter.ScreenId.HasValue)
                return;
            outState.PutString("ScreenId", Adapter.ScreenId.Value.ToString());
        }

        public override void OnAttachFragment(global::Android.Support.V4.App.Fragment p0)
        {
            var screen = p0 as IScreenChanging;
            if (screen != null)
            {
                screen.ScreenChanged += ContentFrameAdapter_ScreenChanged;
            }
            base.OnAttachFragment(p0);
        }
        void ContentFrameAdapter_ScreenChanged(object sender, ScreenChangedEventArgs e)
        {

            var index = Adapter.GetScreenIndex(e.ScreenId);

            if (index >= 0)
            {
                VpContent.CurrentItem = Adapter.GetScreenIndex(e.ScreenId);
                return;
            }

            Adapter.UpdateScreenData(e.ScreenId);
            VpContent.CurrentItem = Adapter.GetScreenIndex(e.ScreenId);
            if (e.ScreenId.HasValue)
            {
                var screen = Model.Screens[e.ScreenId.Value];
                var chapterKey = screen.Breadcrumbs.First();
                for (int i = 0; i < Model.Chapters.Count; i++)
                {
                    if (Model.Chapters[i].ScreenId == chapterKey)
                    {
                        NavList.SelectItem(i);
                    }
                }
            }
            GC.Collect(0);
        }

       
        private void VpContent_PageSelected(object sender, ViewPager.PageSelectedEventArgs e)
        {

            if (Adapter.IsRoot)
                NavList.SelectItem(e.Position);
            var statistic = Adapter.GetItem(e.Position) as StatisticsContentFragment;
            if (statistic != null)
                statistic.RecalculateStatistics();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            VpContent.PageSelected -= VpContent_PageSelected;
            GC.Collect();
        }

        public override void Finish()
        {
            base.Finish();
            
            cleanUpExecutor.CleanUpInterviewCaches(QuestionnaireId);
        }

        public override void OnLowMemory()
        {
            base.OnLowMemory();
            GC.Collect();
        }
    }
}