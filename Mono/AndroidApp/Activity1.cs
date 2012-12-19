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
	public class Activity1 : Activity
	{
		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
       
			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);

	//	    tlSurveys = FindViewById<TableLayout>(Resource.Id.tlSurveys);
		    /*        gridview = FindViewById<GridView>(Resource.Id.gvDashboard);
            var adapter = new DashboardAdapter(this);
            gridview.Adapter = adapter;
            gridview.NumColumns = adapter.ColumnCount;
            gridview.ItemClick += new EventHandler<AdapterView.ItemClickEventArgs>(gridview_ItemClick);
            gridview.Selector=new ColorDrawable(Color.Orange);*/
		}

	 //   protected TableLayout tlSurveys;
	    /* protected GridView gridview;
        void gridview_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            if(e.Position/gridview.NumColumns ==0)
                return;
            
            var intent = new Intent(); 
            intent.SetClass(this, typeof(DetailsActivity));
            intent.PutExtra("questionnaireId", e.View.GetTag(Resource.Id.QuestionnaireId).ToString()); 
         //   intent.PutExtra("attValue", FindViewById(Resource.Id.attVal).ToString()); 
            StartActivity(intent);
        }*/

        protected override void OnResume()
        {
            base.OnResume();
            var main = FindViewById<ScrollView>(Resource.Id.MyLayout);
            main.RemoveAllViews();
          
            var dashboardData =
            CapiApplication.LoadView<DashboardInput, DashboardModel>(new DashboardInput());
            LinearLayout llContainer = new LinearLayout(this);
            llContainer.Orientation=Orientation.Vertical;
            llContainer.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
           llContainer.SetPadding(10,0,10,0);
        /*    var shape = new ShapeDrawable(new RoundRectShape(new float[]{5,5,0,0},new RectF(new Rect()),  ));
            GradientDrawable g = new GradientDrawable();//GradientDrawable.Orientation.BottomTop, new int[] { Color.ParseColor("#6FACD5"), Color.ParseColor("#497BAE") });
         //   g.SetGradientType(GradientType.LinearGradient);
            g.SetCornerRadii(new float[] { 5, 5, 0, 0 });*/
            foreach (var dashboardSurveyItem in dashboardData.Surveys)
            {
                RelativeLayout rlSurvey = new RelativeLayout(this);



                rlSurvey.SetBackgroundResource(Resource.Drawable.SurveyShape);
               // llSurvey.SetBackgroundColor(Color.ParseColor("#5E87B0"));
                var llSurveyLayout = new TableLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent,
                                                                         ViewGroup.LayoutParams.WrapContent);
               
                llSurveyLayout.SetMargins(0,20,0,10);
                
                rlSurvey.LayoutParameters = llSurveyLayout;
                
                

                TextView txtSurveyName = new TextView(this);
                txtSurveyName.Text = dashboardSurveyItem.SurveyTitle;
                txtSurveyName.SetPadding(8, 8, 8, 8);
                txtSurveyName.SetTextColor(Color.White);
                var txtSurveyLayout = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent,
                                                                      ViewGroup.LayoutParams.WrapContent);

                txtSurveyLayout.AddRule(LayoutRules.AlignParentLeft);
             //   txtSurveyLayout.LeftMargin = 20;

                txtSurveyName.LayoutParameters = txtSurveyLayout;
                // txtStatus.SetHeight(80);
                rlSurvey.AddView(txtSurveyName);//, imageWidth, imageHeight);

                TextView txtSurveyCount = new TextView(this);
                txtSurveyCount.Text = dashboardSurveyItem.Items.Count.ToString();
                
                txtSurveyCount.SetTextColor(Color.Black);
                txtSurveyCount.Gravity = GravityFlags.Center;
                var txtSurveyCountLayout = new RelativeLayout.LayoutParams(20,
                                                                      20);
          //      txtSurveyCountLayout.AddRule(LayoutRules.AlignTop, txtSurveyName.Id);
                txtSurveyCountLayout.AddRule(LayoutRules.AlignParentRight);
                txtSurveyCountLayout.AddRule(LayoutRules.CenterVertical);
                txtSurveyCountLayout.RightMargin = 10;
                //     txtSurveyCountLayout.AddRule(LayoutRules.RightOf, txtSurveyName.Id);
            //    txtSurveyCountLayout.SetMargins(5, 0, 5, 0);

               // txtSurveyCount.SetWidth();
                txtSurveyCount.LayoutParameters = txtSurveyCountLayout;
                txtSurveyCount.SetBackgroundResource(Resource.Drawable.CounterRoundShape);
            //    txtSurveyCount.SetWidth(20);
                // txtStatus.SetHeight(80);
                rlSurvey.AddView(txtSurveyCount);//, imageWidth, imageHeight);
                llContainer.AddView(rlSurvey);
                RebuildSurveyView(dashboardSurveyItem, llContainer);
                
            }
            main.AddView(llContainer);

            //LinearLayout layout = FindViewById<LinearLayout>(Resource.Id.MyLayout);
            //foreach (var survey in dashboardData.Groups.SelectMany(g => g.Items))
            //{
            //    var aLabel = new TextView(this);

            //    aLabel.Text = survey.QuestionnaireTitle;
            //    layout.AddView(aLabel);
            //}
        }
        protected void RebuildSurveyView(DashboardSurveyItem survey, LinearLayout parent)
        {/*
           android:layout_width="fill_parent" 
               android:background="#EEEEEE" 
               android:layout_height="wrap_content"
               android:gravity="center"*/
            var tlSurveys = new TableLayout(this);
            tlSurveys.SetPadding(10, 0, 10, 0);
            tlSurveys.LayoutParameters = new TableLayout.LayoutParams
                (ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent);
            tlSurveys.StretchAllColumns = true;
            TableRow th = new TableRow(this);
            th.SetBackgroundResource(Resource.Drawable.SurveyTableHeader);

            for (int c = 0; c < survey.PropertesTitles.Count + 1; c++)
            {
                TextView txtHeader = new TextView(this);
                /*   textView.LayoutParameters = new GridView.LayoutParams(85, 85);
               textView.SetScaleType(ImageView.ScaleType.CenterCrop);*/
                txtHeader.Text = c == 0 ? "Status" : survey.PropertesTitles[c - 1];
                txtHeader.SetPadding(8, 8, 8, 8);
                txtHeader.SetTextColor(Color.ParseColor("#222222"));
                // txtStatus.SetHeight(80);
                th.AddView(txtHeader); //, imageWidth, imageHeight);
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
                    /*   textView.LayoutParameters = new GridView.LayoutParams(85, 85);
                   textView.SetScaleType(ImageView.ScaleType.CenterCrop);*/
                    txtStatus.Text = c == 0 ? survey.Items[r].Status : survey.Items[r].Properties[c - 1];
                    txtStatus.SetPadding(8, 8, 8, 8);
                    txtStatus.SetTextColor(Color.ParseColor("#222222"));
                    
                    // txtStatus.SetHeight(80);
                    tr.AddView(txtStatus);//, imageWidth, imageHeight);
                }
                ImageView ivArrow = new ImageView(this);
                
            /*    var ivArrowLayout = new RelativeLayout.LayoutParams(20, 20);
                ivArrow.LayoutParameters = ivArrowLayout;*/
                ivArrow.SetPadding(0,15,0,0);
                ivArrow.SetImageResource(Android.Resource.Drawable.IcMediaPlay);
             //   ivArrow.LayoutParameters.Width = 20;
                tr.AddView(ivArrow);
                tlSurveys.AddView(tr);
            }
            parent.AddView(tlSurveys);
         //  Android.Resource.Drawable.ButtonPlus
        }

        void tr_Click(object sender, EventArgs e)
        {
            var intent = new Intent();
            intent.SetClass(this, typeof(DetailsActivity));
            intent.PutExtra("questionnaireId", ((TableRow)sender).GetTag(Resource.Id.QuestionnaireId).ToString());
            //   intent.PutExtra("attValue", FindViewById(Resource.Id.attVal).ToString()); 
            StartActivity(intent);
        }
	}
}

