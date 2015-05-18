using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Content.PM;
using Microsoft.Practices.ServiceLocation;
using Ninject;
using WB.Core.BoundedContexts.Capi.ChangeLog;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Capi.Controls;
using WB.UI.Capi.Extensions;
using WB.UI.Capi.Syncronization;
using WB.UI.Capi.ViewModel.Dashboard;

namespace WB.UI.Capi
{
    [Activity(Label = "Interviewer",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class DashboardActivity : Activity
    {
        protected DashboardModel currentDashboard;
        protected IDictionary<Guid,View> sureveyHolders;
        protected LinearLayout llSurveyHolder;
        protected AlertDialog dialog;

        private IChangeLogManipulator logManipulator = CapiApplication.Kernel.Get<IChangeLogManipulator>();
        private IPlainInterviewFileStorage plainInterviewFileStorage = CapiApplication.Kernel.Get<IPlainInterviewFileStorage>();

        private ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

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

                    if (this.currentDashboard.Surveys.Count == 0)
                    {
                        var noAssignmentsMessage = this.CreateNoAssignmentsMessageTextView(this.Resources.GetText(Resource.String.NoAssignments));
                        this.llSurveyHolder.AddView(noAssignmentsMessage);
                    }
                });
        }

        private TextView CreateNoAssignmentsMessageTextView(string messge)
        {
            var layoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent);
            layoutParameters.SetMargins(15, 10, 15, 10);

            var noAssignmentsMessage = new TextView(this)
            {
                Text = messge
            };
            noAssignmentsMessage.SetBackgroundDrawable(this.Resources.GetDrawable(Resource.Drawable.errorwarningstyle));
            noAssignmentsMessage.SetPadding(10, 10, 10, 10);
            noAssignmentsMessage.LayoutParameters = layoutParameters;
            noAssignmentsMessage.SetTextColor(Color.Black);
            noAssignmentsMessage.SetTextSize(ComplexUnitType.Dip, 20);
            return noAssignmentsMessage;
        }

        private void AddSurveyItem(DashboardSurveyItem dashboardSurveyItem)
        {
            var view = this.LayoutInflater.Inflate(Resource.Layout.dashboard_survey_row, null);
            var txtSurveyName = view.FindViewById<TextView>(Resource.Id.txtSurveyName);
            txtSurveyName.Text = string.Format("{0} - v.{1}", dashboardSurveyItem.SurveyTitle, dashboardSurveyItem.QuestionnaireVersion);
            var txtSurveyCount = view.FindViewById<TextView>(Resource.Id.txtSurveyCount);
            txtSurveyCount.Text = dashboardSurveyItem.ActiveItems.Count.ToString(CultureInfo.InvariantCulture);

            var btnNewInterview = view.FindViewById<Button>(Resource.Id.btnNewInterview);
            btnNewInterview.SetTag(Resource.Id.QuestionnaireId, dashboardSurveyItem.QuestionnaireId.ToString());
            btnNewInterview.SetTag(Resource.Id.QuestionnaireVersion, dashboardSurveyItem.QuestionnaireVersion);

            btnNewInterview.Click += this.btnNewInterview_ButtonClick;
            if (!dashboardSurveyItem.AllowCensusMode)
            {
                btnNewInterview.Visibility=ViewStates.Gone;
            }
            
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
                new CapiCleanUpService(logManipulator, this.plainInterviewFileStorage, CapiApplication.Kernel.Get<ISyncPackageIdsStorage>()).DeleteInterview(itemId);
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

            var intent = new Intent(this, typeof(LoadingActivity));
            intent.PutExtra("questionnaireId", target.GetTag(Resource.Id.QuestionnaireId).ToString());
            intent.PutExtra("questionnaireVersion", target.GetTag(Resource.Id.QuestionnaireVersion).ToString());
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