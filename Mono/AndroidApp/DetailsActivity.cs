using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.View;
using Android.Widget;
using AndroidApp.Controls.QuestionnaireDetails;
using AndroidApp.Core;
using AndroidApp.Core.Model.ViewModel.QuestionnaireDetails;
using AndroidApp.Events;
using Main.Core.Entities.SubEntities;
using Ncqrs;
using Ncqrs.Eventing.Storage;

/*
using FragmentTransaction = Android.App.FragmentTransaction;
using Orientation = Android.Content.Res.Orientation;*/

namespace AndroidApp
{
    using System.Linq;

    [Activity(Icon = "@drawable/capi")]
    public class DetailsActivity : MvxSimpleBindingFragmentActivity<CompleteQuestionnaireView>
    {
        protected ItemPublicKey? ScreenId;
        protected InMemoryEventStore activitySnapshooting;
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
        protected QuestionnaireNavigationFragment NavList
        {
            get
            {
                return
                    this.SupportFragmentManager.FindFragmentById(Resource.Id.NavList) as QuestionnaireNavigationFragment;
            }
        }
        
        protected ContentFrameAdapter Adapter { get; set; }

        protected override void OnCreate(Bundle bundle)
        {
            ViewModel = CapiApplication.LoadView<QuestionnaireScreenInput, CompleteQuestionnaireView>(
               new QuestionnaireScreenInput(QuestionnaireId));
            activitySnapshooting = new InMemoryEventStore();
            
            NcqrsEnvironment.SetDefault<ISnapshotStore>(activitySnapshooting);
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Details);
            if (bundle != null)
            {
                var savedScreen = bundle.GetString("ScreenId");
                if (string.IsNullOrEmpty(savedScreen))
                    return;
                ScreenId = ItemPublicKey.Parse(savedScreen);
            }
           
            this.Title = ViewModel.Title;

           if (bundle == null)
            {
                NavList.Model = ViewModel;
                //NavList.SelectItem();
            }

            Adapter = new ContentFrameAdapter(this.SupportFragmentManager, ViewModel, VpContent,
                                              ViewModel.Chapters.FirstOrDefault().ScreenId);
            VpContent.PageSelected += new EventHandler<ViewPager.PageSelectedEventArgs>(VpContent_PageSelected);

        }


        public override void OnAttachFragment(Android.Support.V4.App.Fragment p0)
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
         /*   var firstScreen = CapiApplication.LoadView<QuestionnaireScreenInput, IQuestionnaireViewModel>(
              new QuestionnaireScreenInput(QuestionnaireId, e.ScreenId));*/
          
            Adapter.UpdateScreenData(e.ScreenId);
        }
        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            if (!Adapter.ScreenId.HasValue)
                return;
            outState.PutString("ScreenId", Adapter.ScreenId.Value.ToString());
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
            NcqrsEnvironment.RemoveDefault<ISnapshotStore>();
            activitySnapshooting = null;
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