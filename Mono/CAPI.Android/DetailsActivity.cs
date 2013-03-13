using System;
using Android.App;
using Android.OS;
using Android.Support.V4.View;
using Android.Widget;
using CAPI.Android.Controls.QuestionnaireDetails;
using CAPI.Android.Core;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using System.Linq;
using CAPI.Android.Events;

/*
using FragmentTransaction = Android.App.FragmentTransaction;
using Orientation = Android.Content.Res.Orientation;*/

namespace CAPI.Android
{
    [Activity(Icon = "@drawable/capi")]
    public class DetailsActivity : MvxSimpleBindingFragmentActivity<CompleteQuestionnaireView>
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
        protected LinearLayout llContainer
        {
            get { return this.FindViewById<LinearLayout>(Resource.Id.llContainer); }
        }
     /*   protected LinearLayout llNavigationContainer
        {
            get
            {
                return
                    this.SupportFragmentManager.FindFragmentById(Resource.Id.NavList) as QuestionnaireNavigationFragment;
            }
        }*/
        
        protected ContentFrameAdapter Adapter { get; set; }
        protected QuestionnaireNavigationFragment NavList { get; set; }

        protected override void OnCreate(Bundle bundle)
        {

            if (!CapiApplication.Membership.IsLoggedIn)
            {
                StartActivity(typeof (LoginActivity));
            }


            ViewModel = CapiApplication.LoadView<QuestionnaireScreenInput, CompleteQuestionnaireView>(
                new QuestionnaireScreenInput(QuestionnaireId));

            base.OnCreate(bundle);
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

          /*  if (bundle == null)
            {*/
                NavList = QuestionnaireNavigationFragment.NewInstance(ViewModel.PublicKey);
                this.SupportFragmentManager.BeginTransaction()
                    .Add(Resource.Id.llNavigationContainer, NavList)
                    .Commit();
                //  NavList.NewInstance(ViewModel.PublicKey);
                //NavList.Model = ViewModel;
           /* }
            else
            {
            NavList=
            }*/
            Adapter = new ContentFrameAdapter(this.SupportFragmentManager, ViewModel, VpContent, ScreenId);
            
            VpContent.PageSelected += new EventHandler<ViewPager.PageSelectedEventArgs>(VpContent_PageSelected);

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
            GC.Collect();
        }
        public override void OnLowMemory()
        {
            base.OnLowMemory();
            Console.WriteLine("Low memory Details activities");
            GC.Collect();
        }
    }
}