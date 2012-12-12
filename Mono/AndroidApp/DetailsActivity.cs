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
using Orientation = Android.Content.Res.Orientation;

namespace AndroidApp
{
    [Activity(Label = "My Activity")]
    public class DetailsActivity : Activity 
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

        protected QuestionnaireNavigationFragment NavList { get; set; }
        protected bool DualPanel { get; set; }

        protected override void OnCreate(Bundle bundle)
        {

            base.OnCreate(bundle);
            DualPanel = Resources.Configuration.Orientation
                        == Orientation.Landscape;
            if (DualPanel)
            {
                // If the screen is now in landscape mode, we can show the
                // dialog in-line so we don't need this activity.
                SetContentView(Resource.Layout.Details);
            }
            else
            {
                SetContentView(Resource.Layout.DetailsPortret);
            }
      /*      if (bundle == null)
            {

                
                // During initial setup, plug in the details fragment.
                DetailsFragment details = new DetailsFragment();
                details.setArguments(getIntent().getExtras());
                getSupportFragmentManager().beginTransaction().add(
                        android.R.id.content, details).commit();
            }
            */


            if (FlDetails != null)
            {
                if (bundle != null)
                {
                    return;
                }
                NavList = this.FragmentManager.FindFragmentById<QuestionnaireNavigationFragment>(Resource.Id.NavList);

                NavList.ItemClick += new EventHandler<ScreenChangedEventArgs>(navList_ItemClick);
                NavList.QuestionnaireId = QuestionnaireId;
                if (ScreenId.HasValue)
                {
                    // Make new fragment to show this selection.
                    var details = ScreenContentFragment.NewInstance(this.QuestionnaireId, ScreenId.Value);
                    
                    FragmentManager.BeginTransaction().Add(Resource.Id.flDetails, details).Commit();
                }
                else
                {
                    if (DualPanel)
                    {
                        navList_ItemClick(NavList, new ScreenChangedEventArgs(Guid.Empty));
                    }
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
                intent.SetClass(this, typeof(DetailsActivity));
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