using System;
using System.Threading;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Droid.Views;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Interviewer.ChangeLog;
using WB.Core.BoundedContexts.Interviewer.ErrorReporting.Services.TabletInformationSender;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.UI.Interviewer.Controls;
using WB.UI.Interviewer.Infrastructure.Internals.Security;
using WB.UI.Interviewer.Utils;

namespace WB.UI.Interviewer
{
    [Activity(Label = "Loading", NoHistory = true, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class LoadingActivity : MvxActivity
    {
        private CancellationTokenSource cancellationToken;
        private ISyncPackageRestoreService packageRestoreService = ServiceLocator.Current.GetInstance<ISyncPackageRestoreService>();
        protected ProgressDialog progressDialog;

        private static IStatefulInterviewRepository InterviewRepository
        {
            get { return ServiceLocator.Current.GetInstance<IStatefulInterviewRepository>(); }
        }

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
            if (this.cancellationToken != null && this.cancellationToken.Token.CanBeCanceled)
            {
                this.cancellationToken.Cancel();
            }

            base.Finish();
        }

        protected override void OnStop()
        {
            if (this.cancellationToken != null && this.cancellationToken.Token.CanBeCanceled)
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
                this.Title = this.Resources.GetText(Resource.String.InterviewLoadingError);

                if (!string.IsNullOrEmpty(additionalMessage))
                {
                    this.tvMessage.Text += System.Environment.NewLine +
                        string.Format(this.Resources.GetText(Resource.String.DetailsFormat), additionalMessage);
                }
            });
        }

        protected void CreateNewInterviewOnClient(CancellationToken ct, Guid questionnaireId, long questionnaireVersion)
        {
            var interviewId = Guid.NewGuid();

            var interviewerIdentity = (InterviewerIdentity)Mvx.Resolve<IPrincipal>().CurrentUserIdentity;

            try
            {
                ServiceLocator.Current.GetInstance<ICommandService>()
                    .Execute(new CreateInterviewOnClientCommand(interviewId, interviewerIdentity.UserId,
                        questionnaireId, questionnaireVersion, DateTime.UtcNow, interviewerIdentity.SupervisorId));

                this.LogManipulator.CreatePublicRecord(interviewId);

                if (ct.IsCancellationRequested)
                {
                    return;
                }

                this.NavigationService.NavigateToPrefilledQuestions(interviewId.FormatGuid());
            }
            catch (Exception e)
            {
                this.Logger.Error(e.Message, e);

                if (!ct.IsCancellationRequested)
                {
                    this.ShowErrorMassageToUser(e.Message);
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

                IStatefulInterview interview = InterviewRepository.Get(interviewId.FormatGuid());

                if (ct.IsCancellationRequested)
                {
                    return;
                }

                if (interview == null)
                {
                    this.ShowErrorMassageToUser(string.Format(this.Resources.GetText(Resource.String.InterviewWithIdIsAbsentFormat), interviewId));
                    return;
                }
            }
            catch (Exception e)
            {
                this.Logger.Error(e.Message, e);

                if (!ct.IsCancellationRequested)
                {
                    this.ShowErrorMassageToUser(e.Message);
                }
                return;
            }

            if (ct.IsCancellationRequested)
            {
                return;
            }

            
            if (createdOnClient)
            {
                this.NavigationService.NavigateToPrefilledQuestions(interviewId.FormatGuid());
            }
            else
            {
                this.NavigationService.NavigateToInterview(interviewId.FormatGuid());
            }
        }

        private void btnSendTabletInfo_Click(object sender, EventArgs e)
        {
            this.tvSyncResult.Text = string.Empty;
        }

        void btnSendTabletInfo_ProcessFinished(object sender, EventArgs e)
        {
            this.tvSyncResult.Visibility = ViewStates.Visible;
            this.tvSyncResult.Text = this.Resources.GetText(Resource.String.InformationPackageIsSuccessfullySent) + " " + this.Resources.GetText(Resource.String.ThankYouForPackage);
        }

        private void btnSendTabletInfo_ProcessCanceled(object sender, InformationPackageCancellationEventArgs e)
        {
            this.tvSyncResult.Visibility = ViewStates.Visible;
            this.tvSyncResult.Text = string.Format(this.Resources.GetText(Resource.String.SendingOfInformationPackageIsCanceled), e.Reason);
        }

        private void btnSendTabletInfo_SenderCanceled(object sender, EventArgs e)
        {
            this.tvSyncResult.Text = this.Resources.GetText(Resource.String.SendingOfInformationPackageIsCanceling);
        }
    }
}