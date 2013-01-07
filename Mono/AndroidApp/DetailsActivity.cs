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
using Cirrious.MvvmCross.Binding.Droid.Simple;/*
using FragmentTransaction = Android.App.FragmentTransaction;
using Orientation = Android.Content.Res.Orientation;*/

namespace AndroidApp
{
    [Activity(Icon = "@drawable/capi")]
    public class DetailsActivity : MvxSimpleBindingActivity<IQuestionnaireViewModel> 
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
       /* protected ViewPager VpContent
        {
            get { return this.FindViewById<ViewPager>(Resource.Id.vpContent); }
        }*/
        //protected QuestionnaireNavigationFragment NavList { get; set; }
        protected bool DualPanel { get; set; }
        protected ContentFrameAdapter Adapter { get; set; }
       
        protected override void OnCreate(Bundle bundle)
        {

            base.OnCreate(bundle);
          /*  DualPanel = Resources.Configuration.Orientation
                        == Orientation.Landscape;*/
            ViewModel = CapiApplication.LoadView<QuestionnaireScreenInput, IQuestionnaireViewModel>(
               new QuestionnaireScreenInput(QuestionnaireId, null));
            SetContentView(Resource.Layout.Details);



           
            this.Title = ViewModel.Title;
            

        /*    NavList = this.SupportFragmentManager.FindFragmentById(Resource.Id.NavList) as QuestionnaireNavigationFragment;
            NavList.ItemClick += new EventHandler<ScreenChangedEventArgs>(navList_ItemClick);
            NavList.DataItems = firstScreen.Chapters;*/


           /* Adapter = new ContentFrameAdapter(this.FragmentManager, ViewModel);
            Adapter.ScreenChanged += new EventHandler<ScreenChangedEventArgs>(Adapter_ScreenChanged);
            VpContent.Adapter = Adapter;
            VpContent.PageSelected += new EventHandler<ViewPager.PageSelectedEventArgs>(VpContent_PageSelected);*/
            
        }

    /*    void VpContent_PageSelected(object sender, ViewPager.PageSelectedEventArgs e)
        {
            if (Adapter.IsRoot)
                NavList.SelectItem(e.P0);
        }*/

      /*  void Adapter_ScreenChanged(object sender, ScreenChangedEventArgs e)
        {
            var firstScreen = CapiApplication.LoadView<QuestionnaireScreenInput, IQuestionnaireViewModel>(
                new QuestionnaireScreenInput(QuestionnaireId, e.ScreenId));
           // VpContent.CurrentItem = 0;
            Adapter.UpdateScreenData(firstScreen);
            VpContent.CurrentItem = Adapter.GetScreenIndex(e.ScreenId);
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
        }*/

        protected Guid CurrentScreen { get; set; }

    }
}