using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Content.PM;
using CAPI.Android.Core.Model;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ninject;
using WB.Core.BoundedContexts.Capi.Synchronization.ChangeLog;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.UI.Capi.Controls;
using WB.UI.Capi.Extensions;
using WB.UI.Capi.Settings;
using WB.UI.Capi.Syncronization;

namespace WB.UI.Capi
{
    [Activity(Label = "CAPI",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class DashboardActivity : Activity
    {
        protected DashboardModel currentDashboard;
        protected IDictionary<Guid,View> sureveyHolders;
        protected LinearLayout llSurveyHolder;
        protected AlertDialog dialog;

        private IChangeLogManipulator logManipulator = CapiApplication.Kernel.Get<IChangeLogManipulator>();

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
            txtSurveyCount.Text = dashboardSurveyItem.ActiveItems.Count.ToString(CultureInfo.InvariantCulture);

            var btnNewInterview = view.FindViewById<Button>(Resource.Id.btnNewInterview);
            btnNewInterview.SetTag(Resource.Id.QuestionnaireId, dashboardSurveyItem.QuestionnaireId.ToString());
            btnNewInterview.SetTag(Resource.Id.QuestionnaireVersion, dashboardSurveyItem.QuestionnaireMaxVersion.ToString());

            btnNewInterview.Click += this.btnNewInterview_ButtonClick;
            
            var llQuestionnaireHolder = view.FindViewById<LinearLayout>(Resource.Id.llQuestionnarieHolder);

            var adapter = new DashboardAdapter(dashboardSurveyItem.ActiveItems, this, DeleteInterview);

            for (int i = 0; i < adapter.Count; i++)
            {
                View item = adapter.GetView(i, null, null);
                llQuestionnaireHolder.AddView(item);
                item.Click += this.llQuestionnarieHolder_ItemClick;
            }

            llQuestionnaireHolder.Clickable = true;
            this.llSurveyHolder.AddView(view);
        }


        private void DeleteInterview(Guid itemId, View view)
        {
            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle("Do you want to delete this item?");

            alert.SetPositiveButton("OK", (e, s) =>
            {
                new CapiCleanUpService(logManipulator).DeleteInterveiw(itemId);
                ((LinearLayout)view.Parent).RemoveView(view);
            });

            alert.SetNegativeButton("Cancel", (e, s) =>{});
            
            alert.Show();
        }

        void llQuestionnarieHolder_ItemClick(object sender, EventArgs e)
        {
            var target = sender as View;
            if(target == null)
                return;

            var id = target.GetTag(Resource.Id.IsInterviewLocal);
            bool createdOnClient;

            bool localyCreated = (id != null && bool.TryParse(id.ToString(), out createdOnClient) && createdOnClient);
            
            var intent = new Intent(this, typeof(LoadingActivity));
            intent.PutExtra("publicKey", target.GetTag(Resource.Id.QuestionnaireId).ToString());
            intent.PutExtra("createdOnClient", localyCreated);
            this.StartActivity(intent);
            
        }

        void btnNewInterview_ButtonClick(object sender, EventArgs e)
        {
            var target = sender as Button;
            if (target == null)
                return;

            var questionnaireId = Guid.Parse(target.GetTag(Resource.Id.QuestionnaireId).ToString());
            var questionnaireVersion = long.Parse(target.GetTag(Resource.Id.QuestionnaireVersion).ToString());

            var interviewKey = Guid.NewGuid();

            Guid interviewUserId = CapiApplication.Membership.CurrentUser.Id;
            Guid supervisorId = CapiApplication.Membership.SupervisorId;

            NcqrsEnvironment.Get<ICommandService>().Execute(new CreateInterviewOnClientCommand(interviewKey, interviewUserId,
                questionnaireId, questionnaireVersion, DateTime.UtcNow, supervisorId));

            logManipulator.CreatePublicRecord(interviewKey);
        
            var intent = new Intent(this, typeof(CreateInterviewActivity));
            intent.PutExtra("publicKey", interviewKey.ToString());
            intent.AddFlags(ActivityFlags.NoHistory);
            this.StartActivity(intent);
        }

        private void RequestData(Action restore)
        {
            ThreadPool.QueueUserWorkItem((s) => restore());
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