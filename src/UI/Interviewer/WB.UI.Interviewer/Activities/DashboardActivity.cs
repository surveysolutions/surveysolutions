﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Cirrious.CrossCore;
using Microsoft.Practices.ServiceLocation;
using Ninject;
using WB.Core.BoundedContexts.Interviewer.ChangeLog;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Interviewer.Controls;
using WB.UI.Interviewer.Syncronization;
using WB.UI.Interviewer.ViewModel;
using WB.UI.Interviewer.ViewModel.Dashboard;
using WB.UI.Shared.Enumerator.Activities;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace WB.UI.Interviewer.Activities
{
    [Activity(Theme = "@style/GrayAppTheme")]
    public class DashboardActivity : BaseActivity<DashboardViewModel>
    {
        protected DashboardModel currentDashboard;
        protected IDictionary<Guid,View> sureveyHolders;
        protected LinearLayout llSurveyHolder;
        protected AlertDialog dialog;

        public static ICommandService CommandService
        {
            get { return ServiceLocator.Current.GetInstance<ICommandService>(); }
        }

        private IChangeLogManipulator logManipulator = InterviewerApplication.Kernel.Get<IChangeLogManipulator>();
        private IPlainInterviewFileStorage plainInterviewFileStorage = InterviewerApplication.Kernel.Get<IPlainInterviewFileStorage>();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.SetContentView(Resource.Layout.Main);
            var toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            toolbar.Title = "";
            this.SetSupportActionBar(toolbar);
        }

        protected override void OnStart()
        {
            base.OnStart();

            if (Mvx.Resolve<IDataCollectionAuthentication>().IsLoggedIn)
                this.InitDashboard();
        }

        private void InitDashboard()
        {
            var factory = InterviewerApplication.Kernel.TryGet<IViewFactory<DashboardInput, DashboardModel>>();

            this.currentDashboard = factory == null ? default(DashboardModel) : factory.Load(new DashboardInput(Mvx.Resolve<IDataCollectionAuthentication>().CurrentUser.Id));

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
            var layoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            layoutParameters.SetMargins(15, 10, 15, 10);

            var noAssignmentsMessage = new TextView(this)
            {
                Text = messge
            };
            noAssignmentsMessage.SetBackgroundResource(Resource.Drawable.errorwarningstyle);
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

            var adapter = new DashboardAdapter(dashboardSurveyItem.ActiveItems, this, this.DeleteInterview);

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
                new CapiCleanUpService(this.logManipulator, this.plainInterviewFileStorage, InterviewerApplication.Kernel.Get<ISyncPackageIdsStorage>()).DeleteInterview(itemId);
                ((LinearLayout)view.Parent).RemoveView(view);
            });

            alert.SetNegativeButton("Cancel", (e, s) =>{});
            
            alert.Show();
        }

        private void llQuestionnarieHolder_ItemClick(object sender, EventArgs e)
        {
            var target = sender as View;
            if(target == null)
                return;

            var id = target.GetTag(Resource.Id.IsInterviewLocal);
            bool createdOnClient;

            bool localyCreated = (id != null && bool.TryParse(id.ToString(), out createdOnClient) && createdOnClient);
            var publicKeyTag = target.GetTag(Resource.Id.QuestionnaireId).ToString();
            var interviewId = Guid.Parse(target.GetTag(Resource.Id.QuestionnaireId).ToString());

            var interviews = this.currentDashboard.Surveys.SelectMany(x => x.ActiveItems);
            var selectedInterview = interviews.Single(x => x.PublicKey == interviewId);

            if (selectedInterview.Status == InterviewStatus.Completed)
            {
                var reinitConfirmationDialog = this.CreateYesNoDialog(
                    this,
                    yesHandler: async (s, ev) =>
                    {
                        await CommandService.ExecuteAsync(new RestartInterviewCommand(interviewId, Mvx.Resolve<IDataCollectionAuthentication>().CurrentUser.Id, "", DateTime.UtcNow));
                        this.logManipulator.CreateOrReopenDraftRecord(interviewId, Mvx.Resolve<IDataCollectionAuthentication>().CurrentUser.Id);
                        this.StartLoadingActivity(publicKeyTag, localyCreated);
                    },
                    noHandler: (s, ev) => { },
                    message: this.Resources.GetString(Resource.String.Dashboard_Reinitialize_Interview_Message));

                reinitConfirmationDialog.Show();
            }
            else
            {
                this.StartLoadingActivity(publicKeyTag, localyCreated);
            }
        }

        private void StartLoadingActivity(string interviewId, bool createdOnClient)
        {
            var intent = new Intent(this, typeof(LoadingActivity));
            intent.PutExtra("publicKey", interviewId);
            intent.PutExtra("createdOnClient", createdOnClient);
            this.StartActivity(intent);
        }

        public AlertDialog CreateYesNoDialog(Activity activity, EventHandler<DialogClickEventArgs> yesHandler, EventHandler<DialogClickEventArgs> noHandler, string title = null, string message = null)
        {
            var builder = new AlertDialog.Builder(activity);
            builder.SetNegativeButton(this.Resources.GetString(Resource.String.No), noHandler);
            builder.SetPositiveButton(this.Resources.GetString(Resource.String.Yes), yesHandler);
            builder.SetCancelable(false);
            if (!string.IsNullOrWhiteSpace(title))
            {
                builder.SetTitle(title);
            }
            if (!string.IsNullOrWhiteSpace(message))
            {
                builder.SetMessage(message);
            }
            return builder.Create();
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

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.dashboard, menu);
            return base.OnCreateOptionsMenu(menu);
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_settings:
                    this.ViewModel.NavigateToSettingsCommand.Execute();
                    break;
                case Resource.Id.menu_synchronization:
                    this.ViewModel.NavigateToSynchronizationCommand.Execute();
                    break;

                case Resource.Id.menu_signout:
                    this.ViewModel.SignOutCommand.Execute();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        protected override int ViewResourceId
        {
            get { return Resource.Layout.Main; }
        }
    }
}