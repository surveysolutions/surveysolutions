using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using CAPI.Android.Controls;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using CAPI.Android.Extensions;
using Cirrious.MvvmCross.Binding.Droid.Simple;

namespace CAPI.Android
{
    using global::Android.Content.PM;

    [Activity(Label = "CAPI", Icon = "@drawable/capi",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class
        DashboardActivity : Activity
    {
        protected DashboardModel currentDashboard;
        protected IDictionary<Guid,View> sureveyHolders;
        protected LinearLayout llSurveyHolder;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            if (this.FinishIfNotLoggedIn())
                return;
            sureveyHolders=new Dictionary<Guid, View>();
            SetContentView(Resource.Layout.Main);
             llSurveyHolder = this.FindViewById<LinearLayout>(Resource.Id.llSurveyHolder);
            currentDashboard =
                CapiApplication.LoadView<DashboardInput, DashboardModel>(
                    new DashboardInput(CapiApplication.Membership.CurrentUser.Id));
            foreach (var dashboardSurveyItem in currentDashboard.Surveys)
            {
                AddSurveyItem(dashboardSurveyItem);
            }


        }

        private void AddSurveyItem(DashboardSurveyItem dashboardSurveyItem)
        {
            var view = this.LayoutInflater.Inflate(Resource.Layout.dashboard_survey_row, null);
            var txtSurveyName = view.FindViewById<TextView>(Resource.Id.txtSurveyName);
            txtSurveyName.Text = dashboardSurveyItem.SurveyTitle;
            var txtSurveyCount = view.FindViewById<TextView>(Resource.Id.txtSurveyCount);
            txtSurveyCount.Text = dashboardSurveyItem.ActiveItems.Count.ToString();
            var llQuestionnarieHolder = view.FindViewById<ListView>(Resource.Id.llQuestionnarieHolder);

            llQuestionnarieHolder.Adapter = new DashboardAdapter(this, dashboardSurveyItem.ActiveItems);
            llQuestionnarieHolder.ItemClick += llQuestionnarieHolder_ItemClick;

            llQuestionnarieHolder.Clickable = true;
            llSurveyHolder.AddView(view);
            sureveyHolders.Add(dashboardSurveyItem.PublicKey, view);
        }

        void llQuestionnarieHolder_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var intent = new Intent(this, typeof(LoadingActivity));
            intent.PutExtra("publicKey", e.View.GetTag(Resource.Id.QuestionnaireId).ToString());
            this.StartActivity(intent);
        }
        

        private void RequestData(Action restore)
        {
        /*    var progress = new ProgressDialog(this);
            progress.SetTitle("Loading");
            progress.SetProgressStyle(ProgressDialogStyle.Spinner);
            progress.SetCancelable(false);
            progress.Show();*/

            ThreadPool.QueueUserWorkItem((s) => { restore(); /*progress.Dismiss();*/ });
        }

        protected override void OnRestart()
        {
            base.OnRestart();

            RequestData(UpdateDashboard);
        }

        protected void UpdateDashboard()
        {
            var newDashboard =
                    CapiApplication.LoadView<DashboardInput, DashboardModel>(
                        new DashboardInput(CapiApplication.Membership.CurrentUser.Id));
            var unhandledSurveysKeys = newDashboard.Surveys.Select(s=>s.PublicKey).ToList();
            foreach (var survey in currentDashboard.Surveys)
            {
                
                var newSurvey = newDashboard.Surveys.FirstOrDefault(s => s.PublicKey == survey.PublicKey);
                if (newSurvey == null)
                {
                    this.RunOnUiThread(() =>
                        {
                            llSurveyHolder.RemoveView(sureveyHolders[survey.PublicKey]);
                        });
                    
                    sureveyHolders.Remove(survey.PublicKey);
                    continue;
                }
                unhandledSurveysKeys.Remove(newSurvey.PublicKey);
                if (newSurvey.ActiveItems.Count != survey.ActiveItems.Count)
                {
                    var txtSurveyCount = sureveyHolders[survey.PublicKey].FindViewById<TextView>(Resource.Id.txtSurveyCount);
                    this.RunOnUiThread(() =>
                        {
                            txtSurveyCount.Text = newSurvey.ActiveItems.Count.ToString();
                        });
                }
                var llQuestionnarieHolder = sureveyHolders[survey.PublicKey].FindViewById<ListView>(Resource.Id.llQuestionnarieHolder);
                ((DashboardAdapter) llQuestionnarieHolder.Adapter).Update(newSurvey.ActiveItems);
            }
            foreach (var dashboardSurveyItem in unhandledSurveysKeys)
            {
                this.RunOnUiThread(() =>
                    { AddSurveyItem(newDashboard.Surveys.FirstOrDefault(s => s.PublicKey == dashboardSurveyItem)); });
            }
            currentDashboard = newDashboard;
        }
        
        protected override void OnStart()
        {
            base.OnStart();
            this.CreateActionBar();

        }
    }
}

