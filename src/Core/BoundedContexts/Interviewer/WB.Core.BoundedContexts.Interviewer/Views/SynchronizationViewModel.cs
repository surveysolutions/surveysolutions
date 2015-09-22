using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using Main.Core.Documents;
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
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor;
        private readonly IQuestionnaireModelBuilder questionnaireModelBuilder;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireModelRepository;
        private readonly IPlainQuestionnaireRepository questionnaireRepository;
        private readonly ICommandService commandService;
        private readonly ICapiDataSynchronizationService capiDataSynchronizationService;
        private readonly ISyncPackageIdsStorage syncPackageIdsStorage;
        private readonly ICapiCleanUpService capiCleanUpService;
        private readonly IJsonUtils jsonUtils;
        private readonly IPlainInterviewFileStorage plainInterviewFileStorage;
        private readonly IAsyncPlainStorage<InterviewerIdentity> interviewersPlainStorage;

        private readonly IPasswordHasher passwordHasher;

        private readonly IUserInteractionService userInteractionService;
        private readonly ILogger logger;
        private readonly IPrincipal principal;
        private readonly IFilterableReadSideRepositoryReader<SurveyDto> questionnaireInfoRepository;
        private CancellationTokenSource synchronizationCancellationTokenSource;

        public SynchronizationViewModel(
            ISynchronizationService synchronizationService,
            IViewModelNavigationService viewModelNavigationService,
            IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor,
            IQuestionnaireModelBuilder questionnaireModelBuilder,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireModelRepository,
            IPlainQuestionnaireRepository questionnaireRepository,
            ICommandService commandService,
            ICapiDataSynchronizationService capiDataSynchronizationService,
            ISyncPackageIdsStorage syncPackageIdsStorage,
            ICapiCleanUpService capiCleanUpService,
            IJsonUtils jsonUtils,
            IPlainInterviewFileStorage plainInterviewFileStorage,
            ILogger logger,
            IUserInteractionService userInteractionService, 
            IAsyncPlainStorage<InterviewerIdentity> interviewersPlainStorage, 
            IPasswordHasher passwordHasher,
            IPrincipal principal,
            IFilterableReadSideRepositoryReader<SurveyDto> questionnaireInfoRepository)
        {
            this.synchronizationService = synchronizationService;
            this.viewModelNavigationService = viewModelNavigationService;
            this.questionnaireAssemblyFileAccessor = questionnaireAssemblyFileAccessor;
            this.questionnaireModelBuilder = questionnaireModelBuilder;
            this.questionnaireModelRepository = questionnaireModelRepository;
            this.questionnaireRepository = questionnaireRepository;
            this.commandService = commandService;
            this.capiDataSynchronizationService = capiDataSynchronizationService;
            this.syncPackageIdsStorage = syncPackageIdsStorage;
            this.capiCleanUpService = capiCleanUpService;
            this.jsonUtils = jsonUtils;
            this.plainInterviewFileStorage = plainInterviewFileStorage;
            this.logger = logger;
            this.principal = principal;
            this.userInteractionService = userInteractionService;
            this.interviewersPlainStorage = interviewersPlainStorage;
            this.passwordHasher = passwordHasher;
            this.questionnaireInfoRepository = questionnaireInfoRepository;

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
            
            try
            {
                this.SetProgressOperation(InterviewerUIResources.Synchronization_UserAuthentication_Title,
                    InterviewerUIResources.Synchronization_UserAuthentication_Description);

                await this.synchronizationService.CheckInterviewerCompatibilityWithServerAsync(token: this.Token);

                if (!await this.synchronizationService.IsDeviceLinkedToCurrentInterviewerAsync(token: this.Token, credentials: this.restCredentials))
                {
                    this.viewModelNavigationService.NavigateTo<RelinkDeviceViewModel>(this.principal.CurrentUserIdentity);
                    return;
                }

                if (this.shouldUpdatePasswordOfInterviewer)
                {
                    this.shouldUpdatePasswordOfInterviewer = false;
                    await this.UpdatePasswordOfInterviewerAsync();   
                }

                var packagesByInterviews = await this.synchronizationService.GetInterviewPackagesAsync(
                    this.syncPackageIdsStorage.GetLastStoredPackageId(), this.Token);
                var completedInterviews = this.capiDataSynchronizationService.GetItemsToPush();

                this.statistics.TotalNewInterviewsCount = packagesByInterviews.Interviews.Count(interview => !interview.IsRejected);
                this.statistics.TotalRejectedInterviewsCount = packagesByInterviews.Interviews.Count(interview => interview.IsRejected);
                this.statistics.TotalCompletedInterviewsCount = completedInterviews.Count;

                this.Status = SynchronizationStatus.Upload;
                await this.UploadCompletedInterviewsAsync(completedInterviews);

                this.Status = SynchronizationStatus.Download;
                await this.DownloadCensusAsync();
                await this.DownloadInterviewPackagesAsync(packagesByInterviews);

                this.Status = SynchronizationStatus.Success;
                this.SetProgressOperation(InterviewerUIResources.Synchronization_Success_Title,
                    InterviewerUIResources.Synchronization_Success_Description);
            }
            catch (SynchronizationException ex)
            {
                switch (ex.Type)
                {
                    case SynchronizationExceptionType.RequestCanceledByUser:
                        this.IsSynchronizationInfoShowed = false;
                        break;
                    case SynchronizationExceptionType.Unauthorized:
                        this.shouldUpdatePasswordOfInterviewer = true;
                        break;
                }

                this.Status = SynchronizationStatus.Fail;
                this.SetProgressOperation(InterviewerUIResources.Synchronization_Fail_Title, ex.Message);
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
            }

            if (this.shouldUpdatePasswordOfInterviewer)
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
            var downloadedCensusQuestionnaires = await this.synchronizationService.GetCensusQuestionnairesAsync(this.Token);

            var localCensusQuestionnaires = this.questionnaireInfoRepository.Filter(questionnaire => questionnaire.AllowCensusMode);

            var downloadedCensusQuestionnaireIds = downloadedCensusQuestionnaires.Select(questionnaire => questionnaire.ToString());

            var localCensusQuestionnairesToDelete = localCensusQuestionnaires.Where(
                    questionnaire => !downloadedCensusQuestionnaireIds.Contains(questionnaire.Id)).ToList();

            foreach (var questionnaireInfo in localCensusQuestionnairesToDelete)
            {
                await this.DeleteQuestionnaireAsync(new QuestionnaireIdentity(Guid.Parse(questionnaireInfo.QuestionnaireId), questionnaireInfo.QuestionnaireVersion));   
            }

            var processedQuestionnaires = 0;
            foreach (var censusQuestionnaire in downloadedCensusQuestionnaires)
            {
                this.SetProgressOperation(InterviewerUIResources.Synchronization_Download_Title,
                            InterviewerUIResources.Synchronization_Download_Description_Format.FormatString(
                                processedQuestionnaires, downloadedCensusQuestionnaires.Count,
                                InterviewerUIResources.Synchronization_Questionnaires));

                await this.DownloadQuestionnaireAsync(censusQuestionnaire);

                processedQuestionnaires++;
            }
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

            if (this.questionnaireInfoRepository.GetById(questionnaireIdentity.ToString()) == null)
            {
                var questionnaireApiView = await this.synchronizationService.GetQuestionnaireAsync(
                   questionnaire: questionnaireIdentity,
                   onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                   token: this.Token);

                await this.SaveQuestionnaireAsync(questionnaireIdentity, questionnaireApiView);
                await this.synchronizationService.LogQuestionnaireAsSuccessfullyHandledAsync(questionnaireIdentity);
            }
        }

        private async Task DownloadInterviewPackagesAsync(InterviewPackagesApiView interviewPackages)
        {
            var listOfProcessedInterviews = new List<Guid>();
            foreach (var interviewPackage in interviewPackages.Packages)
            {
                this.SetProgressOperation(InterviewerUIResources.Synchronization_Download_Title,
                    InterviewerUIResources.Synchronization_Download_Description_Format.FormatString(
                        listOfProcessedInterviews.Count, interviewPackages.Interviews.Count,
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
            }
        }

        private async Task UploadCompletedInterviewsAsync(IList<ChangeLogRecordWithContent> dataByChuncks)
        {
            foreach (var chunckDescription in dataByChuncks)
            {
                this.SetProgressOperation(
                    InterviewerUIResources.Synchronization_Upload_Title_Format.FormatString(InterviewerUIResources.Synchronization_Upload_CompletedAssignments_Text),
                    InterviewerUIResources.Synchronization_Upload_Description_Format.FormatString(
                        this.Statistics.CompletedInterviewsCount, this.Statistics.TotalCompletedInterviewsCount,
                        InterviewerUIResources.Synchronization_Upload_Interviews_Text));

                await this.UploadImagesByCompletedInterview(chunckDescription.EventSourceId);

                await this.synchronizationService.UploadInterviewAsync(
                    interviewId: chunckDescription.EventSourceId,
                    content: chunckDescription.Content,
                    onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                    token: this.Token);

                await this.RemoveInterviewAsync(chunckDescription);

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
                this.syncPackageIdsStorage.Append(package.PackageId, synchronizationChunkMeta.SortIndex);
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
                var questionnaireDocument = this.jsonUtils.Deserialize<QuestionnaireDocument>(questionnaireApiView.QuestionnaireDocument);
                var questionnaireModel = this.questionnaireModelBuilder.BuildQuestionnaireModel(questionnaireDocument);
                this.questionnaireModelRepository.Store(questionnaireModel, questionnaireIdentity.ToString());
                this.questionnaireRepository.StoreQuestionnaire(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, questionnaireDocument);
                this.commandService.Execute(new RegisterPlainQuestionnaire(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, questionnaireApiView.AllowCensus, string.Empty));
            });
        }

        public void CancelSynchronizaion()
        {
            this.synchronizationCancellationTokenSource.Cancel();
        }    
    }
}