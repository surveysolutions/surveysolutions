using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using MvvmCross.Plugins.Messenger;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class SynchronizationViewModel : MvxNotifyPropertyChanged
    {
        private readonly ISynchronizationService synchronizationService;
        private readonly IPasswordHasher passwordHasher;
        private readonly IUserInteractionService userInteractionService;
        private readonly ILogger logger;
        private readonly IPrincipal principal;
        private readonly IMvxMessenger messenger;
        private readonly IInterviewerQuestionnaireAccessor questionnaireFactory;
        private readonly IInterviewerInterviewAccessor interviewFactory;

        private readonly IAsyncPlainStorage<InterviewView> interviewViewRepository;
        private readonly IAsyncPlainStorage<InterviewerIdentity> interviewersPlainStorage;
        private readonly IAsyncPlainStorage<InterviewMultimediaView> interviewMultimediaViewStorage;
        private readonly IAsyncPlainStorage<InterviewFileView> interviewFileViewStorage;

        private CancellationTokenSource synchronizationCancellationTokenSource;

        public SynchronizationViewModel(
            IAsyncPlainStorage<InterviewView> interviewViewRepository,
            IAsyncPlainStorage<InterviewerIdentity> interviewersPlainStorage,
            IAsyncPlainStorage<InterviewMultimediaView> interviewMultimediaViewStorage,
            IAsyncPlainStorage<InterviewFileView> interviewFileViewStorage,
            ISynchronizationService synchronizationService,
            ILogger logger,
            IUserInteractionService userInteractionService,
            IPasswordHasher passwordHasher,
            IPrincipal principal,
            IMvxMessenger messenger,
            IInterviewerQuestionnaireAccessor questionnaireFactory,
            IInterviewerInterviewAccessor interviewFactory)
        {
            this.synchronizationService = synchronizationService;
            this.logger = logger;
            this.principal = principal;
            this.userInteractionService = userInteractionService;
            this.interviewersPlainStorage = interviewersPlainStorage;
            this.interviewMultimediaViewStorage = interviewMultimediaViewStorage;
            this.interviewFileViewStorage = interviewFileViewStorage;
            this.passwordHasher = passwordHasher;
            this.interviewViewRepository = interviewViewRepository;
            this.messenger = messenger;
            this.questionnaireFactory = questionnaireFactory;
            this.interviewFactory = interviewFactory;

            this.restCredentials = new RestCredentials()
            {
                Login = this.principal.CurrentUserIdentity.Name,
                Password = this.principal.CurrentUserIdentity.Password
            };
        }

        private readonly RestCredentials restCredentials;
        private CancellationToken Token => this.synchronizationCancellationTokenSource.Token;

        private SychronizationStatistics statistics;
        public SychronizationStatistics Statistics { get { return statistics; } set { statistics = value; this.RaisePropertyChanged(); } }

        private SynchronizationStatus status;
        public SynchronizationStatus Status
        {
            get { return this.status; }
            set { this.status = value; this.RaisePropertyChanged(); }
        }

        private bool isSynchronizationInfoShowed;
        public bool IsSynchronizationInfoShowed
        {
            get { return this.isSynchronizationInfoShowed; }
            set { this.isSynchronizationInfoShowed = value; this.RaisePropertyChanged(); }
        }

        private bool isSynchronizationInProgress;
        public bool IsSynchronizationInProgress
        {
            get { return this.isSynchronizationInProgress; }
            set { this.isSynchronizationInProgress = value; this.RaisePropertyChanged(); }
        }

        private bool hasUserAnotherDevice;
        public bool HasUserAnotherDevice
        {
            get { return this.hasUserAnotherDevice; }
            set { this.hasUserAnotherDevice = value; this.RaisePropertyChanged(); }
        }

        private string processOperation;
        public string ProcessOperation
        {
            get { return this.processOperation; }
            set
            {
                if (this.processOperation == value) return;

                this.processOperation = value;
                this.RaisePropertyChanged();
            }
        }

        public string processOperationDescription;
        public string ProcessOperationDescription
        {
            get { return this.processOperationDescription; }
            set { this.processOperationDescription = value; this.RaisePropertyChanged(); }
        }

        public IMvxCommand CancelSynchronizationCommand => new MvxCommand(this.CancelSynchronizaion);

        bool shouldUpdatePasswordOfInterviewer;
        public async Task SynchronizeAsync()
        {
            this.statistics = new SychronizationStatistics();
            this.synchronizationCancellationTokenSource = new CancellationTokenSource();

            this.Status = SynchronizationStatus.Download;
            this.IsSynchronizationInfoShowed = true;
            this.IsSynchronizationInProgress = true;
            this.messenger.Publish(new SyncronizationStartedMessage(this));

            try
            {
                this.SetProgressOperation(InterviewerUIResources.Synchronization_UserAuthentication_Title,
                    InterviewerUIResources.Synchronization_UserAuthentication_Description);

                await this.synchronizationService.CanSynchronizeAsync(token: this.Token, credentials: this.restCredentials);

                if (this.shouldUpdatePasswordOfInterviewer)
                {
                    this.shouldUpdatePasswordOfInterviewer = false;
                    await this.UpdatePasswordOfInterviewerAsync();   
                }

                this.Status = SynchronizationStatus.Upload;
                await this.UploadCompletedInterviewsAsync();

                this.Status = SynchronizationStatus.Download;
                await this.DownloadCensusAsync();
                await this.DownloadInterviewsAsync();

                this.Status = SynchronizationStatus.Success;
                this.SetProgressOperation(InterviewerUIResources.Synchronization_Success_Title,
                    InterviewerUIResources.Synchronization_Success_Description);
            }
            catch (SynchronizationException ex)
            {
                var errorTitle = InterviewerUIResources.Synchronization_Fail_Title;
                var errorDescription = ex.Message;

                switch (ex.Type)
                {
                    case SynchronizationExceptionType.RequestCanceledByUser:
                        this.IsSynchronizationInfoShowed = false;
                        break;
                    case SynchronizationExceptionType.Unauthorized:
                        this.shouldUpdatePasswordOfInterviewer = true;
                        break;
                    case SynchronizationExceptionType.UserLinkedToAnotherDevice:
                        this.HasUserAnotherDevice = true;
                        errorTitle = InterviewerUIResources.Synchronization_UserLinkedToAnotherDevice_Status;
                        errorDescription = InterviewerUIResources.Synchronization_UserLinkedToAnotherDevice_Title;
                        break;
                }

                this.Status = ex.Type == SynchronizationExceptionType.RequestCanceledByUser 
                    ? SynchronizationStatus.Canceled
                    : SynchronizationStatus.Fail;
                this.SetProgressOperation(errorTitle, errorDescription);
            }
            catch (Exception ex)
            {
                this.Status = SynchronizationStatus.Fail;
                this.SetProgressOperation(InterviewerUIResources.Synchronization_Fail_Title,
                    InterviewerUIResources.Synchronization_Fail_UnexpectedException);
                this.logger.Error("Synchronization. Unexpected exception", ex);
            }
            finally
            {
                this.Statistics = statistics;
                this.IsSynchronizationInProgress = false;
                this.messenger.Publish(new SyncronizationStoppedMessage(this));
            }
            
            if (!this.Token.IsCancellationRequested && this.shouldUpdatePasswordOfInterviewer)
            {
                var newPassword = await this.GetNewPasswordAsync();
                if (newPassword == null)
                {
                    this.shouldUpdatePasswordOfInterviewer = false;
                }
                else
                {
                    this.restCredentials.Password = this.passwordHasher.Hash(newPassword);
                    await SynchronizeAsync();    
                }
            }
        }

        private async Task UpdatePasswordOfInterviewerAsync()
        {
            var localInterviewer = this.interviewersPlainStorage.Query(interviewers => interviewers.FirstOrDefault());
            localInterviewer.Password = this.restCredentials.Password;

            await this.interviewersPlainStorage.StoreAsync(localInterviewer);
            this.principal.SignIn(localInterviewer.Name, localInterviewer.Password, true);
        }

        private async Task<string> GetNewPasswordAsync()
        {
            return await this.userInteractionService.ConfirmWithTextInputAsync(
                message: InterviewerUIResources.Synchronization_UserPassword_Update_Format.FormatString(principal.CurrentUserIdentity.Name),
                okButton: UIResources.LoginText,
                cancelButton: InterviewerUIResources.Synchronization_Cancel, 
                isTextInputPassword: true);
        }

        private void SetProgressOperation(string title, string description)
        {
            this.ProcessOperation = title;
            this.ProcessOperationDescription = description;
        }

        private async Task DownloadCensusAsync()
        {
            var remoteCensusQuestionnaireIdentities = await this.synchronizationService.GetCensusQuestionnairesAsync(this.Token);

            var localCensusQuestionnaireIdentities = this.questionnaireFactory.GetCensusQuestionnaireIdentities();

            var notExistingRemoteCensusQuestionnaireIdentities = localCensusQuestionnaireIdentities.Where(
                    questionnaireIdentity => !remoteCensusQuestionnaireIdentities.Contains(questionnaireIdentity));

            foreach (var censusQuestionnaireIdentity in notExistingRemoteCensusQuestionnaireIdentities)
            {
                await this.questionnaireFactory.RemoveQuestionnaireAsync(censusQuestionnaireIdentity);
            }

            var processedQuestionnaires = 0;
            var notExistingLocalCensusQuestionnaireIdentities = remoteCensusQuestionnaireIdentities.Except(localCensusQuestionnaireIdentities).ToList();
            foreach (var censusQuestionnaireIdentity in notExistingLocalCensusQuestionnaireIdentities)
            {
                this.SetProgressOperation(InterviewerUIResources.Synchronization_Download_Title,
                            InterviewerUIResources.Synchronization_Download_Description_Format.FormatString(
                                processedQuestionnaires, notExistingLocalCensusQuestionnaireIdentities.Count,
                                InterviewerUIResources.Synchronization_Questionnaires));

                await this.DownloadQuestionnaireAsync(censusQuestionnaireIdentity);

                processedQuestionnaires++;
            }
        }

        private async Task DownloadQuestionnaireAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            if (!this.questionnaireFactory.IsQuestionnaireAssemblyExists(questionnaireIdentity))
            {
                var questionnaireAssembly = await this.synchronizationService.GetQuestionnaireAssemblyAsync(
                    questionnaire: questionnaireIdentity,
                    onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                    token: this.Token);

                await this.questionnaireFactory.StoreQuestionnaireAssemblyAsync(questionnaireIdentity, questionnaireAssembly);
                await this.synchronizationService.LogQuestionnaireAssemblyAsSuccessfullyHandledAsync(questionnaireIdentity);
            }
            
            if (!this.questionnaireFactory.IsQuestionnaireExists(questionnaireIdentity))
            {
                var questionnaireApiView = await this.synchronizationService.GetQuestionnaireAsync(
                    questionnaire: questionnaireIdentity,
                    onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                    token: this.Token);

                await this.questionnaireFactory.StoreQuestionnaireAsync(questionnaireIdentity,
                    questionnaireApiView.QuestionnaireDocument, questionnaireApiView.AllowCensus);

                await this.synchronizationService.LogQuestionnaireAsSuccessfullyHandledAsync(questionnaireIdentity);
            }
        }

        private async Task DownloadInterviewsAsync()
        {
            var remoteInterviews = await this.synchronizationService.GetInterviewsAsync(this.Token);

            var remoteInterviewIds = remoteInterviews.Select(interview => interview.Id);

            var localInterviews = await Task.FromResult(this.interviewViewRepository.Query(interviews => interviews.ToList()));

            var localInterviewIds = localInterviews.Select(interview => interview.InterviewId).ToList();

            var localInterviewsToRemove = localInterviews.Where(
                interview => !remoteInterviewIds.Contains(interview.InterviewId) && !interview.CanBeDeleted);

            var localInterviewIdsToRemove = localInterviewsToRemove.Select(interview => interview.InterviewId).ToList();

            var remoteInterviewsToCreate = remoteInterviews.Where(interview => !localInterviewIds.Contains(interview.Id)).ToList();
             
            await this.RemoveInterviewsAsync(localInterviewIdsToRemove);

            await this.CreateInterviewsAsync(remoteInterviewsToCreate);
        }

        private async Task CreateInterviewsAsync(List<InterviewApiView> interviews)
        {
            this.statistics.TotalNewInterviewsCount = interviews.Count(interview => !interview.IsRejected);
            this.statistics.TotalRejectedInterviewsCount = interviews.Count(interview => interview.IsRejected);

            foreach (var interview in interviews)
            {
                this.SetProgressOperation(InterviewerUIResources.Synchronization_Download_Title,
                    InterviewerUIResources.Synchronization_Download_Description_Format.FormatString(
                        this.Statistics.RejectedInterviewsCount + this.Statistics.NewInterviewsCount + 1, interviews.Count,
                        InterviewerUIResources.Synchronization_Interviews));

                await this.DownloadQuestionnaireAsync(interview.QuestionnaireIdentity);

                var interviewDetails = await this.synchronizationService.GetInterviewDetailsAsync(
                    interviewId: interview.Id,
                    onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                    token: this.Token);

                await this.interviewFactory.CreateInterviewAsync(interview, interviewDetails);
                await this.synchronizationService.LogInterviewAsSuccessfullyHandledAsync(interview.Id);

                if (interview.IsRejected)
                    this.Statistics.RejectedInterviewsCount++;
                else
                    this.Statistics.NewInterviewsCount++;
            }
        }

        private async Task RemoveInterviewsAsync(List<Guid> interviewIds)
        {
            this.statistics.TotalDeletedInterviewsCount = interviewIds.Count;

            foreach (var interviewId in interviewIds)
            {
                this.SetProgressOperation(InterviewerUIResources.Synchronization_Download_Title,
                    InterviewerUIResources.Synchronization_Download_Description_Format.FormatString(
                        this.Statistics.DeletedInterviewsCount + 1,
                        interviewIds.Count,
                        InterviewerUIResources.Synchronization_Interviews));

                await this.interviewFactory.RemoveInterviewAsync(interviewId);

                this.Statistics.DeletedInterviewsCount++;
            }
        }

        private async Task UploadCompletedInterviewsAsync()
        {
            var completedInterviews = this.interviewViewRepository.Query(
                    interivews => interivews.Where(interview => interview.Status == InterviewStatus.Completed).ToList());

            this.statistics.TotalCompletedInterviewsCount = completedInterviews.Count;

            foreach (var completedInterview in completedInterviews)
            {
                var jsonPackageByCompletedInterview = await this.interviewFactory.GetPackageByCompletedInterviewAsync(completedInterview.InterviewId);
                this.SetProgressOperation(
                    InterviewerUIResources.Synchronization_Upload_Title_Format.FormatString(InterviewerUIResources.Synchronization_Upload_CompletedAssignments_Text),
                    InterviewerUIResources.Synchronization_Upload_Description_Format.FormatString(
                        this.Statistics.CompletedInterviewsCount, this.Statistics.TotalCompletedInterviewsCount,
                        InterviewerUIResources.Synchronization_Upload_Interviews_Text));

                await this.UploadImagesByCompletedInterview(completedInterview.InterviewId);

                await this.synchronizationService.UploadInterviewAsync(
                    interviewId: completedInterview.InterviewId,
                    content: jsonPackageByCompletedInterview,
                    onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                    token: this.Token);

                await this.interviewFactory.RemoveInterviewAsync(completedInterview.InterviewId);

                this.Statistics.CompletedInterviewsCount++;
            }
        }

        private async Task UploadImagesByCompletedInterview(Guid interviewId)
        {
            var imageViews = this.interviewMultimediaViewStorage.Query(images =>
                images.Where(image => image.InterviewId == interviewId).ToList());

            foreach (var imageView in imageViews)
            {
                var fileView = await Task.FromResult(this.interviewFileViewStorage.GetById(imageView.FileId));
                await this.synchronizationService.UploadInterviewImageAsync(
                    interviewId: imageView.InterviewId,
                    fileName: imageView.FileName,
                    fileData: fileView.File,
                    onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                    token: this.Token);
                await this.interviewMultimediaViewStorage.RemoveAsync(imageView.Id);
                await this.interviewFileViewStorage.RemoveAsync(fileView.Id);
            }
        }

        public void CancelSynchronizaion()
        {
            this.IsSynchronizationInfoShowed = false;
            this.IsSynchronizationInProgress = false;
            if (this.synchronizationCancellationTokenSource != null && !this.synchronizationCancellationTokenSource.IsCancellationRequested)
                this.synchronizationCancellationTokenSource.Cancel();
        }    
    }

    public class SyncronizationStoppedMessage : MvxMessage
    {
        public SyncronizationStoppedMessage(object sender) : base(sender)
        {
        }
    }

    public class SyncronizationStartedMessage : MvxMessage
    {
        public SyncronizationStartedMessage(object sender) : base(sender)
        {
        }
    }
}