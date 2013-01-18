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
using AndroidApp.Controls.Navigation;
using AndroidApp.Controls.QuestionnaireDetails;
using AndroidApp.Controls.QuestionnaireDetails.ScreenItems;
using AndroidApp.Core;
using AndroidApp.Events;
using AndroidApp.Extensions;
using AndroidApp.ViewModel.QuestionnaireDetails;
using Cirrious.MvvmCross.Binding.Droid.Binders;
using Cirrious.MvvmCross.Binding.Droid.Interfaces.Views;
using Cirrious.MvvmCross.Binding.Droid.Simple;
using Cirrious.MvvmCross.Binding.Interfaces;
using Cirrious.MvvmCross.Platform;

/*
using FragmentTransaction = Android.App.FragmentTransaction;
using Orientation = Android.Content.Res.Orientation;*/

namespace AndroidApp
{
    [Activity(Icon = "@drawable/capi")]
    public class DetailsActivity : MvxSimpleBindingFragmentActivity<QuestionnaireScreenViewModel>
    {
       

        protected ItemPublicKey? ScreenId;
        protected Guid QuestionnaireId;
        protected FrameLayout FlDetails
        {
            get { return this.FindViewById<FrameLayout>(Resource.Id.flDetails); }
        }
       
        protected ViewPager VpContent
        {
            get { return this.FindViewById<ViewPager>(Resource.Id.vpContent); }
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
            base.OnCreate(bundle);
            
            SetContentView(Resource.Layout.Details);
            QuestionnaireId = ViewModel.QuestionnaireId;
            if (bundle != null)
            {
                var savedScreen = bundle.GetString("ScreenId");
                if (string.IsNullOrEmpty(savedScreen))
                    return;
                ScreenId = ItemPublicKey.Parse(savedScreen);
            }
            var model = CapiApplication.LoadView<QuestionnaireScreenInput, IQuestionnaireViewModel>(
                new QuestionnaireScreenInput(ViewModel.QuestionnaireId, ScreenId));
            ViewModel = model as QuestionnaireScreenViewModel;
            this.Title = model.Title;

            if (bundle == null)
            {
                NavList.DataItems = model.Chapters;
                NavList.SelectItem(0);
            }

            Adapter = new ContentFrameAdapter(this.SupportFragmentManager, model, VpContent);
            VpContent.PageSelected += new EventHandler<ViewPager.PageSelectedEventArgs>(VpContent_PageSelected);
        }
        
      /*  protected override void OnResume()
        {
            base.OnResume();
            Adapter.NotifyDataSetChanged();
        }
        */
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
            var firstScreen = CapiApplication.LoadView<QuestionnaireScreenInput, IQuestionnaireViewModel>(
              new QuestionnaireScreenInput(QuestionnaireId, e.ScreenId));
            Adapter.UpdateScreenData(firstScreen, e.ScreenId);
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
            Adapter.UpdateScreenData(firstScreen,e.ScreenId);
           /* Adapter = new ContentFrameAdapter(this.SupportFragmentManager, firstScreen, VpContent);
            Adapter.NotifyDataSetChanged();*/
        }

    }
}