using System;
using System.Collections.Generic;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Content.PM;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using WB.UI.Capi.DataCollection.Controls;
using WB.UI.Capi.DataCollection.Extensions;

namespace WB.UI.Capi.DataCollection
{
    [Activity(Label = "CAPI",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class DashboardActivity : Activity
    {
        protected DashboardModel currentDashboard;
        protected IDictionary<Guid,View> sureveyHolders;
        protected LinearLayout llSurveyHolder;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            if (this.FinishIfNotLoggedIn())
                return;
            
            this.SetContentView(Resource.Layout.Main);
            this.InitDashboard();
        }

        private void InitDashboard()
        {
            this.currentDashboard =
                CapiApplication.LoadView<DashboardInput, DashboardModel>(
                    new DashboardInput(CapiApplication.Membership.CurrentUser.Id));

            this.llSurveyHolder = this.FindViewById<LinearLayout>(Resource.Id.llSurveyHolder);
            this.RunOnUiThread(() =>
                {
                    this.llSurveyHolder.RemoveAllViews();

                    foreach (var dashboardSurveyItem in this.currentDashboard.Surveys)
                    {
                        this.AddSurveyItem(dashboardSurveyItem);
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

            var adapter = new DashboardAdapter(dashboardSurveyItem.ActiveItems,this);
            for (int i = 0; i < adapter.Count; i++)
            {
                View item = adapter.GetView(i, null, null);
                llQuestionnarieHolder.AddView(item);
                item.Click += this.llQuestionnarieHolder_ItemClick;
            }

            llQuestionnarieHolder.Clickable = true;
            this.llSurveyHolder.AddView(view);
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

            this.RequestData(this.InitDashboard);
        }
        
        protected override void OnStart()
        {
            base.OnStart();
            this.CreateActionBar();

        }
    }
}

