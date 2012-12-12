using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidApp.Controls.QuestionnaireDetails;
using AndroidApp.ViewModel.QuestionnaireDetails;

namespace AndroidApp
{
    [Activity(Label = "My Activity")]
    public class DetailsActivity : Activity 
    {
        protected Guid QuestionnaireId
        {
            get { return Guid.Parse(Intent.GetStringExtra("questionnaireId")); }
        }
        protected LinearLayout LlContainer
        {
            get { return this.FindViewById<LinearLayout>(Resource.Id.llContainer); }
        }

        protected QuestionnaireNavigationFragment NavList { get; set; }
        protected bool DualPanel { get; set; }

        protected override void OnCreate(Bundle bundle)
        {

            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Details);

            if (LlContainer != null)
            {
                if (bundle != null)
                {
                    return;
                }
                var flDetails = FindViewById<FrameLayout>(Resource.Id.flDetails);
                DualPanel = flDetails != null
                && flDetails.Visibility == ViewStates.Visible;
                NavList = this.FragmentManager.FindFragmentById<QuestionnaireNavigationFragment>(Resource.Id.NavList);
            
                NavList.ItemClick += new EventHandler<ScreenChangedEventArgs>(navList_ItemClick);
                NavList.QuestionnaireId = QuestionnaireId;

       /*         this.FragmentManager.BeginTransaction()
                   .Add(Resource.Id.flContainer, NavList).Commit();*/


                if (DualPanel)
                {
                    navList_ItemClick(NavList, new ScreenChangedEventArgs(Guid.Empty));
                }

            }

            //      this.ScreensContainer = new Dictionary<Guid, ScreenContentFragment>();
            // Set our view from the "main" layout resource

            /*     QuestionnaireNavigationView navList = FindViewById<QuestionnaireNavigationView>(Resource.Id.navList);
            navList.ItemClick += new EventHandler<ScreenChangedEventArgs>(navList_ItemClick);
            navList.QuestionnaireId = QuestionnaireId;
            ScreenContent.QuestionnaireId = QuestionnaireId;
            navList_ItemClick(navList,new ScreenChangedEventArgs(navList.SelectedItem));
            // Create your application here*/
        }

        private void navList_ItemClick(object sender, ScreenChangedEventArgs e)
        {
            if (DualPanel)
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
                intent.SetClass(this, typeof (ScreenContentFragment));
                intent.PutExtra("questionnaireId", this.QuestionnaireId.ToString());
                intent.PutExtra("screenId", e.ScreenId.ToString());
                StartActivity(intent);
            }
            /*  if (ScreensContainer.ContainsKey(CurrentScreen))
            {
                ScreensContainer[CurrentScreen].Visibility = ViewStates.Gone;
            }
            if (ScreensContainer.ContainsKey(e.ScreenId))
            {
                ScreensContainer[e.ScreenId].Visibility = ViewStates.Visible;
            }
            else
            {

                ScreenContentFragment screenFragment = new ScreenContentFragment();
                screenFragment.ScreenId = e.ScreenId;

                Bundle args = new Bundle();
                args.PutInt(ScreenContentFragment.ARG_POSITION, position);
                screenFragment.SetArguments(args);

                ScreensContainer.Add(e.ScreenId, screenFragment);
              this.FragmentManager.PutFragment();

            }
            this.CurrentScreen = e.ScreenId;*/
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