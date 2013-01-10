using System;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Android.Runtime;
using Android.Text;
using Android.Text.Method;
using Android.Views;
using Android.Widget;
using Android.OS;
using AndroidApp.Injections;
using AndroidApp.ViewModel.Input;
using AndroidApp.ViewModel.Model;
using AndroidNcqrs.Eventing.Storage.SQLite;
using Cirrious.MvvmCross.Binding.Droid.Simple;
using Core.CAPI.Views.Grouped;
using Main.Core;
using Main.Core.Events.Questionnaire.Completed;
using Main.Core.View;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using Newtonsoft.Json;
using Ninject;
using Android.Graphics;
namespace AndroidApp
{
	[Activity(Label = "CAPI", MainLauncher = true, Icon = "@drawable/capi")]
    public class Activity1 : MvxSimpleBindingActivity<DashboardModel>
	{
		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
            ViewModel =
                CapiApplication.LoadView<DashboardInput, DashboardModel>(new DashboardInput());
			SetContentView(Resource.Layout.Main);

		}


        protected override void OnResume()
        {
            base.OnResume();
          
       /*     var main = FindViewById<ScrollView>(Resource.Id.MyLayout);
            main.RemoveAllViews();*/
          
           
         /*   LinearLayout llContainer = new LinearLayout(this);
            llContainer.Orientation=Orientation.Vertical;
            llContainer.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
           llContainer.SetPadding(10,0,10,0);*/

         /*   foreach (var dashboardSurveyItem in ViewModel.Surveys)
            {
                //RelativeLayout rlSurvey = new RelativeLayout(this);

                //rlSurvey.SetBackgroundResource(Resource.Drawable.SurveyShape);
                //var llSurveyLayout = new TableLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent,
                //                                                         ViewGroup.LayoutParams.WrapContent);
               
                //llSurveyLayout.SetMargins(0,20,0,10);
                
                //rlSurvey.LayoutParameters = llSurveyLayout;
                
                

          //      TextView txtSurveyName = new TextView(this);
           //     txtSurveyName.Text = dashboardSurveyItem.SurveyTitle;
           //     txtSurveyName.SetPadding(8, 8, 8, 8);
         //       txtSurveyName.SetTextColor(Color.White);
              //  var txtSurveyLayout = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent,
             //                                                         ViewGroup.LayoutParams.WrapContent);

             //   txtSurveyLayout.AddRule(LayoutRules.AlignParentLeft);

           //     txtSurveyName.LayoutParameters = txtSurveyLayout;
            //    rlSurvey.AddView(txtSurveyName);//, imageWidth, imageHeight);

           //     TextView txtSurveyCount = new TextView(this);
             //   txtSurveyCount.Text = dashboardSurveyItem.Items.Count.ToString();
                
              //  txtSurveyCount.SetTextColor(Color.Black);
              //  txtSurveyCount.Gravity = GravityFlags.Center;
             //   var txtSurveyCountLayout = new RelativeLayout.LayoutParams(20,
            //                                                          20);
            //    txtSurveyCountLayout.AddRule(LayoutRules.AlignParentRight);
              //  txtSurveyCountLayout.AddRule(LayoutRules.CenterVertical);
           //     txtSurveyCountLayout.RightMargin = 10;

            //    txtSurveyCount.LayoutParameters = txtSurveyCountLayout;
           //     txtSurveyCount.SetBackgroundResource(Resource.Drawable.CounterRoundShape);
           //     rlSurvey.AddView(txtSurveyCount);
              //  llContainer.AddView(rlSurvey);




                RebuildSurveyView(dashboardSurveyItem, llContainer);
                
            }*/
          //  main.AddView(llContainer);

          
        }
        protected void RebuildSurveyView(DashboardSurveyItem survey, TableLayout tlSurveys)
        {
          /*  var tlSurveys = new TableLayout(this);
            tlSurveys.SetPadding(10, 0, 10, 0);
            tlSurveys.LayoutParameters = new TableLayout.LayoutParams
                (ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent);
            tlSurveys.StretchAllColumns = true;*/
            TableRow th = new TableRow(this);
            th.SetBackgroundResource(Resource.Drawable.SurveyTableHeader);

            for (int c = 0; c < survey.PropertesTitles.Count + 1; c++)
            {
                TextView txtHeader = new TextView(this);
                txtHeader.Text = c == 0 ? "Status" : survey.PropertesTitles[c - 1];
                txtHeader.SetPadding(8, 8, 8, 8);
                txtHeader.SetTextColor(Color.ParseColor("#222222"));
                th.AddView(txtHeader);
            }

            TextView txtEmpty = new TextView(this);
            txtEmpty.SetWidth(10);
            th.AddView(txtEmpty);
            tlSurveys.AddView(th);

            for (int r = 0; r < survey.Items.Count; r++)
            {
                TableRow tr = new TableRow(this);
                tr.SetMinimumHeight(60);
                tr.SetTag(Resource.Id.QuestionnaireId, survey.Items[r].PublicKey.ToString());
              
                tr.SetBackgroundResource(Resource.Drawable.cell_shape);
                tr.Click += tr_Click;
                for (int c = 0; c < survey.Items[r].Properties.Count + 1; c++)
                {
                    TextView txtStatus = new TextView(this);
                    txtStatus.Text = c == 0 ? survey.Items[r].Status : survey.Items[r].Properties[c - 1];
                    txtStatus.SetPadding(8, 8, 8, 8);
                    txtStatus.SetTextColor(Color.ParseColor("#222222"));
                    
                    tr.AddView(txtStatus);
                }
                ImageView ivArrow = new ImageView(this);
                ivArrow.SetPadding(0,15,0,0);
                ivArrow.SetImageResource(Android.Resource.Drawable.IcMediaPlay);
                tr.AddView(ivArrow);
                tlSurveys.AddView(tr);
            }
           // parent.AddView(tlSurveys);
        }

        void tr_Click(object sender, EventArgs e)
        {
            var intent = new Intent();
            intent.SetClass(this, typeof(DetailsActivity));
            intent.PutExtra("questionnaireId", ((TableRow)sender).GetTag(Resource.Id.QuestionnaireId).ToString());
            StartActivity(intent);
        }
	}
}

