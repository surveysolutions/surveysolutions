using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using Main.Core.Documents;
using MvvmCross.Plugins.Messenger;
using WB.Core.BoundedContexts.Interviewer.ChangeLog;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.UI.Interviewer.ViewModel.Dashboard;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class SynchronizationViewModel : MvxNotifyPropertyChanged
    {
        private readonly ISynchronizationService synchronizationService;
        private readonly IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor;
        private readonly IQuestionnaireModelBuilder questionnaireModelBuilder;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireModelRepository;
        private readonly IPlainQuestionnaireRepository questionnaireRepository;
        private readonly ICommandService commandService;
        private readonly ICapiDataSynchronizationService capiDataSynchronizationService;
        private readonly IInterviewPackageIdsStorage interviewPackageIdsStorage;
        private readonly ICapiCleanUpService capiCleanUpService;
        private readonly ISerializer serializer;
        private readonly IPlainInterviewFileStorage plainInterviewFileStorage;
        private readonly IAsyncPlainStorage<InterviewerIdentity> interviewersPlainStorage;

        private readonly IPasswordHasher passwordHasher;

        private readonly IUserInteractionService userInteractionService;
        private readonly ILogger logger;
        private readonly IPrincipal principal;
        private readonly IFilterableReadSideRepositoryReader<SurveyDto> questionnaireInfoRepository;
        private readonly IFilterableReadSideRepositoryReader<QuestionnaireDTO> interviewInfoRepository;
        private CancellationTokenSource synchronizationCancellationTokenSource;
        private IMvxMessenger messenger;

        public SynchronizationViewModel(
            ISynchronizationService synchronizationService,
            IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor,
            IQuestionnaireModelBuilder questionnaireModelBuilder,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireModelRepository,
            IPlainQuestionnaireRepository questionnaireRepository,
            ICommandService commandService,
            ICapiDataSynchronizationService capiDataSynchronizationService,
            IInterviewPackageIdsStorage interviewPackageIdsStorage,
            ICapiCleanUpService capiCleanUpService,
            ISerializer serializer,
            IPlainInterviewFileStorage plainInterviewFileStorage,
            ILogger logger,
            IUserInteractionService userInteractionService, 
            IAsyncPlainStorage<InterviewerIdentity> interviewersPlainStorage, 
            IPasswordHasher passwordHasher,
            IPrincipal principal,
            IFilterableReadSideRepositoryReader<SurveyDto> questionnaireInfoRepository,
            IFilterableReadSideRepositoryReader<QuestionnaireDTO> interviewInfoRepository, 
            IMvxMessenger messenger)
        {
            this.synchronizationService = synchronizationService;
            this.questionnaireAssemblyFileAccessor = questionnaireAssemblyFileAccessor;
            this.questionnaireModelBuilder = questionnaireModelBuilder;
            this.questionnaireModelRepository = questionnaireModelRepository;
            this.questionnaireRepository = questionnaireRepository;
            this.commandService = commandService;
            this.capiDataSynchronizationService = capiDataSynchronizationService;
            this.interviewPackageIdsStorage = interviewPackageIdsStorage;
            this.capiCleanUpService = capiCleanUpService;
            this.serializer = serializer;
            this.plainInterviewFileStorage = plainInterviewFileStorage;
            this.logger = logger;
            this.principal = principal;
            this.userInteractionService = userInteractionService;
            this.interviewersPlainStorage = interviewersPlainStorage;
            this.passwordHasher = passwordHasher;
            this.questionnaireInfoRepository = questionnaireInfoRepository;
            this.interviewInfoRepository = interviewInfoRepository;
            this.messenger = messenger;

            this.restCredentials = new RestCredentials()
            {
                Login = this.principal.CurrentUserIdentity.Name,
                Password = this.principal.CurrentUserIdentity.Password
            };
        }

        private RestCredentials restCredentials;
        private CancellationToken Token { get { return this.synchronizationCancellationTokenSource.Token; } }

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

        public IMvxCommand CancelSynchronizationCommand
        {
            get { return new MvxCommand(this.CancelSynchronizaion); }
        }

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
                await this.DownloadInterviewPackagesAsync();

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

            var localCensusQuestionnaireIdentities = this.questionnaireInfoRepository.Filter(questionnaire => questionnaire.AllowCensusMode)
                .Select(questionnaire => new QuestionnaireIdentity(Guid.Parse(questionnaire.QuestionnaireId), questionnaire.QuestionnaireVersion)).ToList();

            var notExistingRemoteCensusQuestionnaireIdentities = localCensusQuestionnaireIdentities.Where(
                    questionnaireIdentity => !remoteCensusQuestionnaireIdentities.Contains(questionnaireIdentity));
            foreach (var censusQuestionnaireIdentity in notExistingRemoteCensusQuestionnaireIdentities)
            {
                await this.DeleteInterviewsByQuestionnaireAsync(censusQuestionnaireIdentity);
                await this.DeleteQuestionnaireAsync(censusQuestionnaireIdentity);
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

        private async Task DeleteInterviewsByQuestionnaireAsync(QuestionnaireIdentity censusQuestionnaireIdentity)
        {
            await Task.Run(() =>
            {
                var formattedQuestionnaireId = censusQuestionnaireIdentity.QuestionnaireId.FormatGuid();
                var interviewIdsByQuestionnaire = this.interviewInfoRepository
                    .Filter(interview => interview.Survey == formattedQuestionnaireId &&
                                         interview.SurveyVersion == censusQuestionnaireIdentity.Version)
                    .Select(interview => Guid.Parse(interview.Id))
                    .ToList();

                foreach (var interviewId in interviewIdsByQuestionnaire)
                {
                    this.capiCleanUpService.DeleteInterview(interviewId);
                }
            });
        }

        private async Task DownloadQuestionnaireAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            if (!this.questionnaireAssemblyFileAccessor.IsQuestionnaireAssemblyExists(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version))
            {
                var questionnaireAssembly = await this.synchronizationService.GetQuestionnaireAssemblyAsync(
                    questionnaire: questionnaireIdentity,
                    onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                    token: this.Token);

                this.questionnaireAssemblyFileAccessor.StoreAssembly(questionnaireIdentity.QuestionnaireId,
                    questionnaireIdentity.Version, questionnaireAssembly);
                await this.synchronizationService.LogQuestionnaireAssemblyAsSuccessfullyHandledAsync(questionnaireIdentity);
            }

            var formattedQuestionnaireId = questionnaireIdentity.QuestionnaireId.FormatGuid();
            if (!this.questionnaireInfoRepository.Filter(questionnaire =>
                        questionnaire.QuestionnaireId == formattedQuestionnaireId &&
                        questionnaire.QuestionnaireVersion == questionnaireIdentity.Version).Any())
            {
                var questionnaireApiView = await this.synchronizationService.GetQuestionnaireAsync(
                    questionnaire: questionnaireIdentity,
                    onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                    token: this.Token);

                await this.SaveQuestionnaireAsync(questionnaireIdentity, questionnaireApiView);
                await this.synchronizationService.LogQuestionnaireAsSuccessfullyHandledAsync(questionnaireIdentity);
            }
        }

        private async Task DownloadInterviewPackagesAsync()
        {
            var interviewPackages = await this.synchronizationService.GetInterviewPackagesAsync(
                    this.interviewPackageIdsStorage.GetLastStoredPackageId(), this.Token);

            this.statistics.TotalNewInterviewsCount = interviewPackages.Interviews.Count(interview => !interview.IsRejected);
            this.statistics.TotalRejectedInterviewsCount = interviewPackages.Interviews.Count(interview => interview.IsRejected);
            this.statistics.TotalDeletedInterviewsCount = interviewPackages.Packages.Count(interview => interview.ItemType == SyncItemType.DeleteInterview);

            var listOfProcessedInterviews = new List<Guid>();
            foreach (var interviewPackage in interviewPackages.Packages.OrderBy(package => package.SortIndex))
            {
                this.SetProgressOperation(InterviewerUIResources.Synchronization_Download_Title,
                    InterviewerUIResources.Synchronization_Download_Description_Format.FormatString(
                        listOfProcessedInterviews.Count, interviewPackages.Packages.Count,
                        InterviewerUIResources.Synchronization_Interviews));

                var interviewInfo = interviewPackages.Interviews.Find(interview => interview.Id == interviewPackage.InterviewId);
                if (interviewInfo != null)
                {
                    await this.DownloadQuestionnaireAsync(interviewInfo.QuestionnaireIdentity);
                };

                var package = await this.synchronizationService.GetInterviewPackageAsync(
                    packageId: interviewPackage.Id,
                    onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                    token: this.Token);

                await this.SaveInterviewAsync(package, interviewPackage);
                await this.synchronizationService.LogPackageAsSuccessfullyHandledAsync(interviewPackage.Id);

                if (!listOfProcessedInterviews.Contains(interviewPackage.InterviewId)) listOfProcessedInterviews.Add(interviewPackage.InterviewId);

                if (interviewInfo != null)
                {
                    if (interviewInfo.IsRejected)
                        this.Statistics.RejectedInterviewsCount++;
                    else
                        this.Statistics.NewInterviewsCount++;
                }
                else
                {
                    this.Statistics.DeletedInterviewsCount++;
                }
            }
        }

        private async Task UploadCompletedInterviewsAsync()
        {
            var completedInterviews = this.capiDataSynchronizationService.GetItemsToPush();
            this.statistics.TotalCompletedInterviewsCount = completedInterviews.Count;

            foreach (var completedInterview in completedInterviews)
            {
                this.SetProgressOperation(
                    InterviewerUIResources.Synchronization_Upload_Title_Format.FormatString(InterviewerUIResources.Synchronization_Upload_CompletedAssignments_Text),
                    InterviewerUIResources.Synchronization_Upload_Description_Format.FormatString(
                        this.Statistics.CompletedInterviewsCount, this.Statistics.TotalCompletedInterviewsCount,
                        InterviewerUIResources.Synchronization_Upload_Interviews_Text));

                await this.UploadImagesByCompletedInterview(completedInterview.EventSourceId);

                await this.synchronizationService.UploadInterviewAsync(
                    interviewId: completedInterview.EventSourceId,
                    content: completedInterview.Content,
                    onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                    token: this.Token);

                await this.RemoveInterviewAsync(completedInterview);

                this.Statistics.CompletedInterviewsCount++;
            }
        }

        private async Task UploadImagesByCompletedInterview(Guid interviewId)
        {
            await Task.Run(async () =>
            {
                var interviewImages =  this.plainInterviewFileStorage.GetBinaryFilesForInterview(interviewId);
                foreach (var image in interviewImages)
                {
                    await this.synchronizationService.UploadInterviewImageAsync(
                        interviewId: image.InterviewId,
                        fileName: image.FileName,
                        fileData: image.GetData(),
                        onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                        token: this.Token);
                    this.plainInterviewFileStorage.RemoveInterviewBinaryData(image.InterviewId, image.FileName);
                }
            });
        }

        private async Task DeleteQuestionnaireAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            await Task.Run(() =>
            {
                    this.commandService.Execute(new DisableQuestionnaire(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, null));
                    this.questionnaireRepository.DeleteQuestionnaireDocument(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);
                    this.questionnaireAssemblyFileAccessor.RemoveAssembly(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);
                    this.questionnaireModelRepository.Remove(questionnaireIdentity.ToString());

                    this.commandService.Execute(new DeleteQuestionnaire(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, null));
                
            });
        }

        private async Task SaveInterviewAsync(InterviewSyncPackageDto package, SynchronizationChunkMeta synchronizationChunkMeta)
        {
            await Task.Run(() =>
            {
                this.capiDataSynchronizationService.ProcessDownloadedInterviewPackages(package, synchronizationChunkMeta.ItemType);
                this.interviewPackageIdsStorage.Store(package.PackageId, synchronizationChunkMeta.SortIndex);
            });
        }

        private async Task RemoveInterviewAsync(ChangeLogRecordWithContent chunckDescription)
        {
            await Task.Run(() =>
            {
                this.capiCleanUpService.DeleteInterview(chunckDescription.EventSourceId);
            });
        }

        private async Task SaveQuestionnaireAsync(QuestionnaireIdentity questionnaireIdentity, QuestionnaireApiView questionnaireApiView)
        {
            await Task.Run(() =>
            {
                var questionnaireDocument = this.serializer.Deserialize<QuestionnaireDocument>(questionnaireApiView.QuestionnaireDocument);
                var questionnaireModel = this.questionnaireModelBuilder.BuildQuestionnaireModel(questionnaireDocument);
                this.questionnaireModelRepository.Store(questionnaireModel, questionnaireIdentity.ToString());
                this.questionnaireRepository.StoreQuestionnaire(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, questionnaireDocument);
                this.commandService.Execute(new RegisterPlainQuestionnaire(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, questionnaireApiView.AllowCensus, string.Empty));
            });
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