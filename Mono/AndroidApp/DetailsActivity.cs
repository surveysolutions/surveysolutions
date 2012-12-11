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

        protected override void OnCreate(Bundle bundle)
        {

            base.OnCreate(bundle);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Details);
            
          /*QuestionnaireNavigationView navigationView=new QuestionnaireNavigationView(this,new XmlReaderResourceParser() );
            LinearLayout layout = FindViewById<LinearLayout>(Resource.Id.MyLayout);
            layout.AddView(tvId);*/
            QuestionnaireNavigationView navList = FindViewById<QuestionnaireNavigationView>(Resource.Id.navList);
            navList.ItemClick += new EventHandler<ScreenChangedEventArgs>(navList_ItemClick);
            navList.QuestionnaireId = QuestionnaireId;
            

            
            ScreenContent.QuestionnaireId = QuestionnaireId;
            // Create your application here
        }

        void navList_ItemClick(object sender, ScreenChangedEventArgs e)
        {
            ScreenContent.ScreenId = e.ScreenId;
        }

        protected ScreenContentView ScreenContent
        {
            get { return FindViewById<ScreenContentView>(Resource.Id.scveenView); }
        }

    }
}