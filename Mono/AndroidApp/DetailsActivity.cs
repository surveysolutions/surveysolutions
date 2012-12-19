using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using AndroidApp.Controls.QuestionnaireDetails;
using AndroidApp.ViewModel.QuestionnaireDetails;
using FragmentTransaction = Android.App.FragmentTransaction;
using Orientation = Android.Content.Res.Orientation;

namespace AndroidApp
{
    [Activity(Icon = "@drawable/capi")]
    public class DetailsActivity : FragmentActivity 
    {
        protected Guid QuestionnaireId
        {
            get { return Guid.Parse(Intent.GetStringExtra("questionnaireId")); }
        }
        protected Guid? ScreenId
        {
            get
            {
                var scrId = Intent.GetStringExtra("screenId");
                if (string.IsNullOrEmpty(scrId))
                    return null;
                return Guid.Parse(scrId);
            }
        }
        protected FrameLayout FlDetails
        {
            get { return this.FindViewById<FrameLayout>(Resource.Id.flDetails); }
        }
        protected ViewPager VpContent
        {
            get { return this.FindViewById<ViewPager>(Resource.Id.vpContent); }
        }
        protected QuestionnaireNavigationFragment NavList { get; set; }
        protected bool DualPanel { get; set; }
        protected ContentFrameAdapter Adapter { get; set; }
       
        protected override void OnCreate(Bundle bundle)
        {

            base.OnCreate(bundle);
            DualPanel = Resources.Configuration.Orientation
                        == Orientation.Landscape;
            SetContentView(Resource.Layout.Details);



            var firstScreen = CapiApplication.LoadView<QuestionnaireScreenInput, IQuestionnaireViewModel>(
                new QuestionnaireScreenInput(QuestionnaireId, null));
            this.Title = firstScreen.Title;
            

            NavList = this.SupportFragmentManager.FindFragmentById(Resource.Id.NavList) as QuestionnaireNavigationFragment;
            NavList.ItemClick += new EventHandler<ScreenChangedEventArgs>(navList_ItemClick);
            NavList.DataItems = firstScreen.Chapters;
            

            Adapter = new ContentFrameAdapter(SupportFragmentManager,firstScreen);
            Adapter.ScreenChanged += new EventHandler<ScreenChangedEventArgs>(Adapter_ScreenChanged);
            VpContent.Adapter = Adapter;
            

        }

        void Adapter_ScreenChanged(object sender, ScreenChangedEventArgs e)
        {
            var firstScreen = CapiApplication.LoadView<QuestionnaireScreenInput, IQuestionnaireViewModel>(
                new QuestionnaireScreenInput(QuestionnaireId, e.ScreenId));
            VpContent.CurrentItem = 0;
            Adapter.UpdateScreenData(firstScreen);
            VpContent.CurrentItem = Adapter.GetScreenIndex(e.ScreenId);
           /* Adapter = new ContentFrameAdapter(SupportFragmentManager, firstScreen);
            VpContent.Invalidate();*/
      //      VpContent.Adapter = Adapter;
         //   VpContent.CurrentItem = Adapter.GetScreenIndex(e.ScreenId);
        }

        private void navList_ItemClick(object sender, ScreenChangedEventArgs e)
        {
            var index = Adapter.GetScreenIndex(e.ScreenId);

            if (index >= 0)
            {
                VpContent.CurrentItem = Adapter.GetScreenIndex(e.ScreenId);
                return;
            }
            var firstScreen = CapiApplication.LoadView<QuestionnaireScreenInput, IQuestionnaireViewModel>(
                new QuestionnaireScreenInput(QuestionnaireId, e.ScreenId));
            Adapter.UpdateScreenData(firstScreen);
            VpContent.CurrentItem = Adapter.GetScreenIndex(e.ScreenId);
        }

        protected Guid CurrentScreen { get; set; }

    }
}