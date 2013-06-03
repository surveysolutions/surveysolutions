using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using CAPI.Android.Controls.QuestionnaireDetails;
using CAPI.Android.Core;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using System.Linq;
using CAPI.Android.Events;
using CAPI.Android.Extensions;
using Main.Core.Domain;
using Ncqrs.Restoring.EventStapshoot;

namespace CAPI.Android
{
    [Activity(NoHistory = true, Icon = "@drawable/capi", ConfigurationChanges = ConfigChanges.Orientation |ConfigChanges.KeyboardHidden |ConfigChanges.ScreenSize)]
    public class DetailsActivity : MvxSimpleBindingFragmentActivity<CompleteQuestionnaireView>/*, View.IOnTouchListener*/
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
        protected ViewPager VpContent
        {
            get { return this.FindViewById<ViewPager>(Resource.Id.vpContent); }
        }
        protected LinearLayout llNavigationHolder
        {
            get { return this.FindViewById<LinearLayout>(Resource.Id.llNavigationHolder); }
        }
        protected RelativeLayout llContainer
        {
            get { return this.FindViewById<RelativeLayout>(Resource.Id.llContainer); }
        }
        
        protected ContentFrameAdapter Adapter { get; set; }
        protected QuestionnaireNavigationFragment NavList { get; set; }

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
                ScreenId = ViewModel.Chapters.FirstOrDefault().ScreenId;
            }

            this.Title = ViewModel.Title;
            
            if (bundle == null)
            {
                NavList = QuestionnaireNavigationFragment.NewInstance(ViewModel.PublicKey);
                this.SupportFragmentManager.BeginTransaction()
                    .Add(Resource.Id.llNavigationHolder, NavList, "navigation")
                    .Commit();
            }
            else
            {
                NavList = this.SupportFragmentManager.FindFragmentByTag("navigation") as QuestionnaireNavigationFragment;
            }
            llNavigationHolder.BringToFront();
            llNavigationHolder.Click += llNavigationHolder_Click;
            Adapter = new ContentFrameAdapter(this.SupportFragmentManager, ViewModel, ScreenId);
            VpContent.Adapter = Adapter;
            VpContent.PageSelected += VpContent_PageSelected;

            llNavigationHolder.SetBackgroundColor(this.Resources.GetColor(global::Android.Resource.Color.DarkerGray));
            
            var llNavigationContainerParams =
                new RelativeLayout.LayoutParams(this.WindowManager.DefaultDisplay.Width/2,
                                                ViewGroup
                                                    .LayoutParams
                                                    .FillParent);
            llNavigationContainerParams.LeftMargin = llNavigationHolder.PaddingRight - llNavigationContainerParams.Width;
            llNavigationHolder.LayoutParameters = llNavigationContainerParams;


            var VpContentParams =
               new RelativeLayout.LayoutParams(this.WindowManager.DefaultDisplay.Width - llNavigationHolder.PaddingRight,
                                               ViewGroup
                                                   .LayoutParams
                                                   .FillParent);
            VpContentParams.LeftMargin = llNavigationHolder.PaddingRight;
            VpContent.LayoutParameters = VpContentParams;
        }

        private bool isChaptersVisible = false;

        private void llNavigationHolder_Click(object sender, EventArgs e)
        {
            int right, left;
            if (isChaptersVisible)
            {
                right = llNavigationHolder.PaddingRight;
                left = llNavigationHolder.PaddingRight - this.WindowManager.DefaultDisplay.Width / 2;
                isChaptersVisible = false;
            }
            else
            {
                right = this.WindowManager.DefaultDisplay.Width/2;
                left = 0;
                isChaptersVisible = true;
            }
            ((RelativeLayout.LayoutParams) llNavigationHolder.LayoutParameters).LeftMargin = left;
            ((RelativeLayout.LayoutParams) VpContent.LayoutParameters).LeftMargin = right;

            llNavigationHolder.Layout(left, llNavigationHolder.Top, right, llNavigationHolder.Bottom);

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
        public override void Finish()
        {
            CapiApplication.CommandService.Execute(new CreateSnapshotForAR(QuestionnaireId, typeof(CompleteQuestionnaireAR)));
            base.Finish();
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
                var screen = ViewModel.Screens[e.ScreenId.Value];
                var chapterKey = screen.Breadcrumbs.First();
                for (int i = 0; i < ViewModel.Chapters.Count; i++)
                {
                    if (ViewModel.Chapters[i].ScreenId == chapterKey)
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
                NavList.SelectItem(e.P0);
            var statistic = Adapter.GetItem(e.P0) as StatisticsContentFragment;
            if (statistic != null)
                statistic.RecalculateStatistics();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            VpContent.PageSelected -= VpContent_PageSelected;
            GC.Collect();
        }


        public override void OnLowMemory()
        {
            base.OnLowMemory();
            GC.Collect();
        }
    }
}