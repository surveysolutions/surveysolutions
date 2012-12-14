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
    [Activity(Label = "My Activity")]
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



            var firstScreen = CapiApplication.LoadView<QuestionnaireScreenInput, QuestionnaireScreenViewModel>(
                new QuestionnaireScreenInput(QuestionnaireId, null, null));
            NavList = this.SupportFragmentManager.FindFragmentById(Resource.Id.NavList) as QuestionnaireNavigationFragment;
            NavList.ItemClick += new EventHandler<ScreenChangedEventArgs>(navList_ItemClick);
            NavList.DataItems = firstScreen.Chapters;


            Adapter = new ContentFrameAdapter(SupportFragmentManager,firstScreen);
            VpContent.Adapter = Adapter;
        
            /*  if (ScreenId.HasValue)
              {
                  // Make new fragment to show this selection.
                  var details = ScreenContentFragment.NewInstance(this.QuestionnaireId, ScreenId.Value);
                    
                  Android.App.FragmentManager.BeginTransaction().Add(Resource.Id.flDetails, details).Commit();
              }
              else
              {
                  if (DualPanel)
                  {
                      navList_ItemClick(NavList, new ScreenChangedEventArgs(Guid.Empty));
                  }
              }*/




        }
        
        private void navList_ItemClick(object sender, ScreenChangedEventArgs e)
        {
            if (!e.ScreenId.HasValue)
            {
                VpContent.CurrentItem = VpContent.Adapter.Count - 1;
            }
        //    VpContent.CurrentItem = 1;
            /*   if (DualPanel)
            {
                ScreenContentFragment details =
                    this.FragmentManager.FindFragmentById<ScreenContentFragment>(Resource.Id.flDetails);
                if (details == null || details.ScreenId != e.ScreenId)
                {
                    // Make new fragment to show this selection.
                    details = ScreenContentFragment.NewInstance(this.QuestionnaireId, e.ScreenId);

                    // Execute a transaction, replacing any existing
                    // fragment with this one inside the frame.
                    FragmentTransaction ft
                            = this.FragmentManager.BeginTransaction();
                    ft.Replace(Resource.Id.flDetails, details);
                    ft.SetTransition(
                            FragmentTransit.FragmentFade);
                    ft.Commit();
                }
            }
            else
            {
                Intent intent = new Intent();
                intent.SetClass(this, typeof(DetailsActivity));
                intent.PutExtra("questionnaireId", this.QuestionnaireId.ToString());
                intent.PutExtra("screenId", e.ScreenId.ToString());
                StartActivity(intent);
            }*/
        }
        
        protected Guid CurrentScreen { get; set; }

       /* protected ScreenContentView ScreenContent
        {
            get { return FindViewById<ScreenContentView>(Resource.Id.scveenView); }
        }*/

    /*    protected LinearLayout ContentLayout
        {
            get { return FindViewById<LinearLayout>(Resource.Id.contentLayout); }
        }*/
     //   protected IDictionary<Guid, ScreenContentFragment> ScreensContainer;



    }
}