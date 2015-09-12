using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.ViewModels;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Interviewer.ChangeLog;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

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
        private readonly IInterviewSynchronizationFileStorage interviewSynchronizationFileStorage;
        private readonly ICapiCleanUpService capiCleanUpService;
        private readonly IPrincipal principal;
        private readonly IJsonUtils jsonUtils;
        private readonly IAsyncPlainStorage<CensusQuestionnireInfo> plainStorageQuestionnireCensusInfo;
        private CancellationTokenSource synchronizationCancellationTokenSource;

        public SynchronizationViewModel(
            SychronizationStatistics statistics,
            ISynchronizationService synchronizationService,
            IViewModelNavigationService viewModelNavigationService,
            IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor,
            IQuestionnaireModelBuilder questionnaireModelBuilder,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireModelRepository,
            IPlainQuestionnaireRepository questionnaireRepository,
            ICommandService commandService,
            ICapiDataSynchronizationService capiDataSynchronizationService,
            ISyncPackageIdsStorage syncPackageIdsStorage,
            IInterviewSynchronizationFileStorage interviewSynchronizationFileStorage,
            ICapiCleanUpService capiCleanUpService,
            IPrincipal principal,
            IJsonUtils jsonUtils,
            IAsyncPlainStorage<CensusQuestionnireInfo> plainStorageQuestionnireCensusInfo)
        {
            this.Statistics = statistics;
            this.synchronizationService = synchronizationService;
            this.viewModelNavigationService = viewModelNavigationService;
            this.questionnaireAssemblyFileAccessor = questionnaireAssemblyFileAccessor;
            this.questionnaireModelBuilder = questionnaireModelBuilder;
            this.questionnaireModelRepository = questionnaireModelRepository;
            this.questionnaireRepository = questionnaireRepository;
            this.commandService = commandService;
            this.capiDataSynchronizationService = capiDataSynchronizationService;
            this.syncPackageIdsStorage = syncPackageIdsStorage;
            this.interviewSynchronizationFileStorage = interviewSynchronizationFileStorage;
            this.capiCleanUpService = capiCleanUpService;
            this.principal = principal;
            this.jsonUtils = jsonUtils;
            this.plainStorageQuestionnireCensusInfo = plainStorageQuestionnireCensusInfo;
        }

        private CancellationToken Token { get { return this.synchronizationCancellationTokenSource.Token; } }
        public SychronizationStatistics Statistics { get; set; }

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

        public async Task SynchronizeAsync()
        {
            this.synchronizationCancellationTokenSource = new CancellationTokenSource();

            this.Status = SynchronizationStatus.Download;
            this.IsSynchronizationInfoShowed = true;
            this.IsSynchronizationInProgress = true;
            try
            {
                this.SetProgressOperation(InterviewerUIResources.Synchronization_UserAuthentication_Title,
                    InterviewerUIResources.Synchronization_UserAuthentication_Description);

                await this.synchronizationService.CheckInterviewerCompatibilityWithServerAsync(this.Token);

                if (!await this.synchronizationService.IsDeviceLinkedToCurrentInterviewerAsync(this.Token))
                {
                    this.viewModelNavigationService.NavigateTo<RelinkDeviceViewModel>();
                    return;
                }

                this.Status = SynchronizationStatus.Upload;
                await this.UploadCompletedInterviewsAsync();
                await this.UploadImagesByCompletedInterviewsAsync();

                this.Status = SynchronizationStatus.Download;
                await this.DownloadQuestionnairesByInterviewsAsync();
                await this.DownloadInterviewPackagesAsync();

                this.Status = SynchronizationStatus.Success;
                this.SetProgressOperation(InterviewerUIResources.Synchronization_Success_Title,
                    InterviewerUIResources.Synchronization_Success_Description);
            }
            catch (RestException ex)
            {
                switch (ex.Type)
                {
                        case RestExceptionType.RequestCancelled:
                        this.IsSynchronizationInfoShowed = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                Mvx.Trace(MvxTraceLevel.Error, ex.Message);
                this.Status = SynchronizationStatus.Fail;
                this.SetProgressOperation(InterviewerUIResources.Synchronization_Fail_Title,
                    InterviewerUIResources.Synchronization_Fail_Description);
            }
            finally
            {
                this.IsSynchronizationInProgress = false;
            }
        }

        private void SetProgressOperation(string title, string description)
        {
            this.ProcessOperation = title;
            this.ProcessOperationDescription = description;
        }

        private async Task DownloadQuestionnairesByInterviewsAsync()
        {
            var censusQuestionnaires = await this.synchronizationService.GetCensusQuestionnairesAsync(this.Token);
            var interviews = await this.synchronizationService.GetInterviewsAsync(this.Token);

            var questionnairesByInterviews = censusQuestionnaires.Union(interviews.Select(interview => interview.QuestionnaireIdentity)).Distinct();

            await this.DownloadQuestionnairesAsync(questionnairesByInterviews);
            await this.DownloadQuestionnaireAssembliesAsync(questionnairesByInterviews);

            foreach (var interviewApiView in interviews)
            {
                #warning NEW SYNCHRONIZATION CODE BY INTERVIEWS SHOULD BE HERE
            }
        }

        private async Task DownloadQuestionnaireAssembliesAsync(IEnumerable<QuestionnaireIdentity> questionnairesByInterviews)
        {
            var questionnaireAssembliesToDownload = questionnairesByInterviews
                .Where(questionnaireIdentity => !this.questionnaireAssemblyFileAccessor.IsQuestionnaireAssemblyExists(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version))
                .ToList();

            foreach (var questionnaireIdentity in questionnaireAssembliesToDownload)
            {
                this.SetProgressOperation(InterviewerUIResources.Synchronization_Download_Title,
                    InterviewerUIResources.Synchronization_Download_Description_Format.FormatString(
                        this.Statistics.DownloadedQuestionnaireAssemliesCount++, questionnaireAssembliesToDownload.Count,
                        InterviewerUIResources.Synchronization_Conditions_Validations));

                var questionnaireAssembly = await this.synchronizationService.GetQuestionnaireAssemblyAsync(
                    questionnaire: questionnaireIdentity,
                    onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                    token: this.Token);

                this.questionnaireAssemblyFileAccessor.StoreAssembly(questionnaireIdentity.QuestionnaireId,
                    questionnaireIdentity.Version, questionnaireAssembly);
            }
        }

        private async Task DownloadQuestionnairesAsync(IEnumerable<QuestionnaireIdentity> questionnairesByInterviews)
        {
            var questionnairesToDownload = questionnairesByInterviews
                .Where(questionnaireIdentity => this.questionnaireModelRepository.GetById(questionnaireIdentity.ToString()) == null)
                .ToList();

            foreach (var questionnaireIdentity in questionnairesToDownload)
            {
                this.SetProgressOperation(InterviewerUIResources.Synchronization_Download_Title,
                            InterviewerUIResources.Synchronization_Download_Description_Format.FormatString(
                                this.Statistics.DownloadedQuestionnairesCount++, questionnairesToDownload.Count,
                                InterviewerUIResources.Synchronization_Questionnaires));

                var questionnaireApiView = await this.synchronizationService.GetQuestionnaireAsync(
                    questionnaire: questionnaireIdentity,
                    onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                    token: this.Token);

                await this.SaveQuestionnaireAsync(questionnaireIdentity, questionnaireApiView);
            }
        }

        private async Task DownloadInterviewPackagesAsync()
        {
            var lastKnownPackageId = this.syncPackageIdsStorage.GetLastStoredPackageId();
            var interviewPackages = await this.synchronizationService.GetInterviewPackagesAsync(lastKnownPackageId, this.Token);

            foreach (var synchronizationChunkMeta in interviewPackages)
            {
                this.SetProgressOperation(InterviewerUIResources.Synchronization_Download_Title,
                    InterviewerUIResources.Synchronization_Download_Description_Format.FormatString(
                        this.Statistics.DownloadedInterviewsCount++, interviewPackages.Count,
                        InterviewerUIResources.Synchronization_Interviews));

                var package = await this.synchronizationService.GetInterviewPackageAsync(
                    packageId: synchronizationChunkMeta.Id,
                    previousSuccessfullyHandledPackageId: lastKnownPackageId,
                    onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                    token: this.Token);

                lastKnownPackageId = synchronizationChunkMeta.Id;

                await this.SaveInterviewAsync(package, synchronizationChunkMeta);
            }

            await this.synchronizationService.LogPackageAsSuccessfullyHandledAsync(lastKnownPackageId, this.Token);
        }

        private async Task UploadCompletedInterviewsAsync()
        {
            var dataByChuncks = this.capiDataSynchronizationService.GetItemsToPush();
            foreach (var chunckDescription in dataByChuncks)
            {
                this.SetProgressOperation(
                    InterviewerUIResources.Synchronization_Upload_Title_Format.FormatString(InterviewerUIResources.Synchronization_Upload_CompletedAssignments_Text),
                    InterviewerUIResources.Synchronization_Upload_Description_Format.FormatString(
                        this.Statistics.UploadedInterviewsCount++, dataByChuncks.Count,
                        InterviewerUIResources.Synchronization_Upload_Interviews_Text));

                await this.synchronizationService.UploadInterviewAsync(
                    interviewId: chunckDescription.EventSourceId,
                    content: chunckDescription.Content,
                    onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                    token: this.Token);

                await this.RemoveInterviewAsync(chunckDescription);
            }
        }

        private async Task UploadImagesByCompletedInterviewsAsync()
        {
            var images = this.interviewSynchronizationFileStorage.GetImagesByInterviews();
            foreach (var image in images)
            {
                this.SetProgressOperation(
                    InterviewerUIResources.Synchronization_Upload_Title_Format.FormatString(
                        InterviewerUIResources.Synchronization_Upload_ImagesByInterviews_Text),
                    InterviewerUIResources.Synchronization_Upload_Description_Format.FormatString(
                        this.Statistics.UploadedIterviewImagesCount++, images.Count,
                        InterviewerUIResources.Synchronization_Upload_Images_Text));
                
                await this.synchronizationService.UploadInterviewImageAsync(
                    interviewId: image.InterviewId,
                    fileName: image.FileName,
                    fileData: image.GetData(),
                    onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                    token: this.Token);

                this.interviewSynchronizationFileStorage.RemoveInterviewImage(image.InterviewId, image.FileName);
            }
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
                this.interviewSynchronizationFileStorage.MoveInterviewImagesToSyncFolder(chunckDescription.EventSourceId);
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

            if (questionnaireApiView.AllowCensus)
            {
                await this.plainStorageQuestionnireCensusInfo.StoreAsync(new CensusQuestionnireInfo() {Id = questionnaireIdentity.ToString()});
            }
        }

        public void CancelSynchronizaion()
        {
            this.synchronizationCancellationTokenSource.Cancel();
        }    
    }
}