using System;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Droid.Views;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Capi.ChangeLog;
using WB.Core.BoundedContexts.Capi.ErrorReporting.Services.TabletInformationSender;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.UI.Capi.Controls;
using WB.UI.Capi.ViewModel;
using WB.UI.Shared.Android.Helpers;
using InterviewViewModel = WB.Core.BoundedContexts.Capi.Views.InterviewDetails.InterviewViewModel;

namespace WB.UI.Capi
{
    [Activity(Label = "Loading", NoHistory = true, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class LoadingActivity : MvxActivity
    {
        private CancellationTokenSource cancellationToken;
        private ISyncPackageRestoreService packageRestoreService = ServiceLocator.Current.GetInstance<ISyncPackageRestoreService>();
        protected ProgressDialog progressDialog;

        protected TabletInformationReportButton btnSendTabletInfo
        {
            get { return this.FindViewById<TabletInformationReportButton>(Resource.Id.btnSendTabletInfo); }
        }

        protected TextView tvMessage
        {
            get { return this.FindViewById<TextView>(Resource.Id.tvMessage); }
        }

        protected TextView tvSyncResult
        {
            get { return this.FindViewById<TextView>(Resource.Id.tvSyncResult); }
        }

        private ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

        private IChangeLogManipulator LogManipulator
        {
            get { return ServiceLocator.Current.GetInstance<IChangeLogManipulator>(); }
        }

        private IViewModelNavigationService NavigationService
        {
            get { return ServiceLocator.Current.GetInstance<IViewModelNavigationService>(); }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.ActionBar.SetDisplayShowHomeEnabled(false);

            var interviewIdString = this.Intent.GetStringExtra("publicKey");
            if (!string.IsNullOrEmpty(interviewIdString))
            {
                var interviewId = Guid.Parse(interviewIdString);
                var createdOnClient = this.Intent.GetBooleanExtra("createdOnClient", false);

                this.cancellationToken =
                    this.WaitForLongOperation(
                        (ct) => this.Restore(ct, interviewId, createdOnClient));
                return;
            }

            var questionnaireIdString = this.Intent.GetStringExtra("questionnaireId");
            if (!string.IsNullOrEmpty(questionnaireIdString))
            {
                var questionnaireId = Guid.Parse(questionnaireIdString);
                var questionnaireVersion = long.Parse(this.Intent.GetStringExtra("questionnaireVersion"));

                this.cancellationToken =
                    this.WaitForLongOperation(
                        (ct) => this.CreateNewInterviewOnClient(ct, questionnaireId, questionnaireVersion));
            }
        }
        
        public override void OnBackPressed()
        {
            this.RunOnUiThread(this.Finish);
            base.OnBackPressed();
        }

        public override void Finish()
        {
            if (this.cancellationToken != null && cancellationToken.Token.CanBeCanceled)
            {
                this.cancellationToken.Cancel();
            }

            base.Finish();
        }

        protected override void OnStop()
        {
            if (this.cancellationToken != null && cancellationToken.Token.CanBeCanceled)
            {
                this.cancellationToken.Cancel();
            }
            base.OnStop();
        }

        protected void ShowErrorMassageToUser(string additionalMessage)
        {
            this.RunOnUiThread(() =>
            {
                this.cancellationToken = null;
                this.SetContentView(Resource.Layout.ErrorScreen);
                this.btnSendTabletInfo.Click += this.btnSendTabletInfo_Click;
                this.btnSendTabletInfo.ProcessFinished += this.btnSendTabletInfo_ProcessFinished;
                this.btnSendTabletInfo.ProcessCanceled += this.btnSendTabletInfo_ProcessCanceled;
                this.btnSendTabletInfo.SenderCanceled += this.btnSendTabletInfo_SenderCanceled;
                this.ActionBar.SetDisplayShowHomeEnabled(true);
                this.Title = Resources.GetText(Resource.String.InterviewLoadingError);

                if (!string.IsNullOrEmpty(additionalMessage))
                {
                    this.tvMessage.Text += System.Environment.NewLine +
                        string.Format(Resources.GetText(Resource.String.DetailsFormat), additionalMessage);
                }
            });
        }

        protected void CreateNewInterviewOnClient(CancellationToken ct, Guid questionnaireId, long questionnaireVersion)
        {
            var interviewId = Guid.NewGuid();

            Guid interviewUserId = CapiApplication.Membership.CurrentUser.Id;
            Guid supervisorId = CapiApplication.Membership.SupervisorId;

            try
            {
                ServiceLocator.Current.GetInstance<ICommandService>()
                    .Execute(new CreateInterviewOnClientCommand(interviewId, interviewUserId,
                        questionnaireId, questionnaireVersion, DateTime.UtcNow, supervisorId));

                LogManipulator.CreatePublicRecord(interviewId);

                if (ct.IsCancellationRequested)
                {
                    return;
                }

                NavigationService.NavigateTo<InterviewerPrefilledQuestionsViewModel>(new { interviewId = interviewId.FormatGuid() });
            }
            catch (Exception e)
            {
                Logger.Error(e.Message, e);

                if (!ct.IsCancellationRequested)
                {
                    ShowErrorMassageToUser(e.Message);
                }
            }

        }

        protected void Restore(CancellationToken ct, Guid interviewId, bool createdOnClient)
        {
            try
            {
                this.packageRestoreService.CheckAndApplySyncPackage(interviewId);

                if (ct.IsCancellationRequested)
                {
                    return;
                }

                InterviewViewModel interview = CapiApplication.LoadView<QuestionnaireScreenInput, InterviewViewModel>(
                    new QuestionnaireScreenInput(interviewId));

                if (ct.IsCancellationRequested)
                {
                    return;
                }

                if (interview == null)
                {
                    ShowErrorMassageToUser(string.Format(Resources.GetText(Resource.String.InterviewWithIdIsAbsentFormat), interviewId));
                    return;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message, e);

                if (!ct.IsCancellationRequested)
                {
                    ShowErrorMassageToUser(e.Message);
                }
                return;
            }

            if (ct.IsCancellationRequested)
            {
                return;
            }

            
            if (createdOnClient)
            {
                NavigationService.NavigateTo<InterviewerPrefilledQuestionsViewModel>(new { interviewId = interviewId.FormatGuid() });
            }
            else
            {
                NavigationService.NavigateTo<InterviewerInterviewViewModel>(new { interviewId = interviewId.FormatGuid() });
            }
        }

        private void btnSendTabletInfo_Click(object sender, EventArgs e)
        {
            this.tvSyncResult.Text = string.Empty;
        }

        void btnSendTabletInfo_ProcessFinished(object sender, EventArgs e)
        {
            this.tvSyncResult.Visibility = ViewStates.Visible;
            this.tvSyncResult.Text = Resources.GetText(Resource.String.InformationPackageIsSuccessfullySent) + " " + Resources.GetText(Resource.String.ThankYouForPackage);
        }

        private void btnSendTabletInfo_ProcessCanceled(object sender, InformationPackageCancellationEventArgs e)
        {
            this.tvSyncResult.Visibility = ViewStates.Visible;
            this.tvSyncResult.Text = string.Format(Resources.GetText(Resource.String.SendingOfInformationPackageIsCanceled), e.Reason);
        }

        private void btnSendTabletInfo_SenderCanceled(object sender, EventArgs e)
        {
            this.tvSyncResult.Text = Resources.GetText(Resource.String.SendingOfInformationPackageIsCanceling);
        }
    }
}