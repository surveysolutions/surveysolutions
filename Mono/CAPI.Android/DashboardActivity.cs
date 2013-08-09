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
            
            SetContentView(Resource.Layout.Main);
            InitDashboard();
        }

        private void InitDashboard()
        {
            currentDashboard =
                CapiApplication.LoadView<DashboardInput, DashboardModel>(
                    new DashboardInput(CapiApplication.Membership.CurrentUser.Id));
            sureveyHolders = new Dictionary<Guid, View>();
            llSurveyHolder = this.FindViewById<LinearLayout>(Resource.Id.llSurveyHolder);
            this.RunOnUiThread(() =>
                {
                    llSurveyHolder.RemoveAllViews();

                    foreach (var dashboardSurveyItem in currentDashboard.Surveys)
                    {
                        AddSurveyItem(dashboardSurveyItem);
                    }
                });
        }

        private void AddSurveyItem(DashboardSurveyItem dashboardSurveyItem)
        {
            var view = this.LayoutInflater.Inflate(Resource.Layout.dashboard_survey_row, null);
            var txtSurveyName = view.FindViewById<TextView>(Resource.Id.txtSurveyName);
            txtSurveyName.Text = dashboardSurveyItem.SurveyTitle;
            var txtSurveyCount = view.FindViewById<TextView>(Resource.Id.txtSurveyCount);
            txtSurveyCount.Text = dashboardSurveyItem.ActiveItems.Count.ToString();
            var llQuestionnarieHolder = view.FindViewById<LinearLayout>(Resource.Id.llQuestionnarieHolder);

            var adapter = new DashboardAdapter(this, dashboardSurveyItem.ActiveItems);
            for (int i = 0; i < adapter.Count; i++)
            {
                View item = adapter.GetView(i, null, null);
                llQuestionnarieHolder.AddView(item);
                item.Click += llQuestionnarieHolder_ItemClick;
            }

            llQuestionnarieHolder.Clickable = true;
            llSurveyHolder.AddView(view);
            sureveyHolders.Add(dashboardSurveyItem.PublicKey, view);
        }

        void llQuestionnarieHolder_ItemClick(object sender, EventArgs e)
        {
            var target = sender as View;
            if(target==null)
                return;
            var intent = new Intent(this, typeof(LoadingActivity));
            intent.PutExtra("publicKey", target.GetTag(Resource.Id.QuestionnaireId).ToString());
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

            RequestData(InitDashboard);
        }

        protected void UpdateDashboard()
        {
            currentDashboard =
              CapiApplication.LoadView<DashboardInput, DashboardModel>(
                  new DashboardInput(CapiApplication.Membership.CurrentUser.Id));
            sureveyHolders = new Dictionary<Guid, View>();
            llSurveyHolder = this.FindViewById<LinearLayout>(Resource.Id.llSurveyHolder);
            this.RunOnUiThread(() =>
            {
                llSurveyHolder.RemoveAllViews();

                foreach (var dashboardSurveyItem in currentDashboard.Surveys)
                {
                    AddSurveyItem(dashboardSurveyItem);
                }
            });
        }
        
        protected override void OnStart()
        {
            base.OnStart();
            this.CreateActionBar();

        }
    }
}

