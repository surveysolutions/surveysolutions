using System;
using System.Threading;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
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
        private readonly IAsyncPlainStorage<CensusQuestionnireInfo> plainStorageQuestionnireCensusInfo;
        private readonly CancellationTokenSource synchronizationCancellationTokenSource = new CancellationTokenSource();

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
            IInterviewSynchronizationFileStorage interviewSynchronizationFileStorage,
            ICapiCleanUpService capiCleanUpService,
            IPrincipal principal,
            IAsyncPlainStorage<CensusQuestionnireInfo> plainStorageQuestionnireCensusInfo)
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
            this.interviewSynchronizationFileStorage = interviewSynchronizationFileStorage;
            this.capiCleanUpService = capiCleanUpService;
            this.principal = principal;
            this.plainStorageQuestionnireCensusInfo = plainStorageQuestionnireCensusInfo;
        }

        private bool isSynchronizationInProgress;
        public bool IsSynchronizationInProgress
        {
            get { return this.isSynchronizationInProgress; }
            set { this.isSynchronizationInProgress = value; this.RaisePropertyChanged(); }
        }

        public async Task SynchronizeAsync()
        {
            this.IsSynchronizationInProgress = true;
            try
            {
                await this.synchronizationService.CheckInterviewerCompatibilityWithServerAsync();

                if (!await this.synchronizationService.IsDeviceLinkedToCurrentInterviewerAsync())
                {
                    this.viewModelNavigationService.NavigateTo<RelinkDeviceViewModel>();
                    return;
                }

                await this.UploadCompletedInterviewsAsync();
                await this.UploadImagesByCompletedInterviewsAsync();
                await this.DownloadCensusQuestionnairesAsync();
                await this.DownloadInterviewsAsync();
                await this.DownloadInterviewPackagesAsync();
            }
            catch (Exception ex)
            {
                Mvx.Trace(MvxTraceLevel.Error, ex.Message);
            }
            finally
            {
                this.IsSynchronizationInProgress = false;
            }
        }

        private async Task DownloadCensusQuestionnairesAsync()
        {
            var censusQuestionnaires = await this.synchronizationService.GetCensusQuestionnairesAsync();

            foreach (var censusQuestionnaire in censusQuestionnaires)
            {
                await this.DownloadQuestionnaireIfDoesNotExistsAsync(censusQuestionnaire);
            }
        }

        private async Task DownloadInterviewsAsync()
        {
            var interviews = await this.synchronizationService.GetInterviewsAsync();

            foreach (var interview in interviews)
            {
                await this.DownloadQuestionnaireIfDoesNotExistsAsync(interview.QuestionnaireIdentity);

                #warning NEW SYNCHRONIZATION CODE BY INTERVIEWS SHOULD BE HERE
            }
        }

        private async Task DownloadInterviewPackagesAsync()
        {
            var lastKnownPackageId = this.syncPackageIdsStorage.GetLastStoredPackageId();
            var interviewPackages = await this.synchronizationService.GetInterviewPackagesAsync(lastKnownPackageId);

            foreach (var synchronizationChunkMeta in interviewPackages)
            {
                var package = await this.synchronizationService.GetInterviewPackageAsync(
                    packageId: synchronizationChunkMeta.Id,
                    previousSuccessfullyHandledPackageId: lastKnownPackageId,
                    onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                    token: synchronizationCancellationTokenSource.Token);

                lastKnownPackageId = synchronizationChunkMeta.Id;

                this.capiDataSynchronizationService.ProcessDownloadedPackage(package, synchronizationChunkMeta.ItemType);
                this.syncPackageIdsStorage.Append(package.PackageId, synchronizationChunkMeta.SortIndex);
            }

            await this.synchronizationService.LogPackageAsSuccessfullyHandledAsync(lastKnownPackageId);
        }

        private async Task UploadCompletedInterviewsAsync()
        {
            var dataByChuncks = this.capiDataSynchronizationService.GetItemsToPush();
            foreach (var chunckDescription in dataByChuncks)
            {
                await this.synchronizationService.UploadInterviewAsync(
                    interviewId: chunckDescription.EventSourceId,
                    content: chunckDescription.Content,
                    onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                    token: synchronizationCancellationTokenSource.Token);

                this.interviewSynchronizationFileStorage.MoveInterviewImagesToSyncFolder(chunckDescription.EventSourceId);
                this.capiCleanUpService.DeleteInterview(chunckDescription.EventSourceId);
            }
        }

        private async Task UploadImagesByCompletedInterviewsAsync()
        {
            var images = this.interviewSynchronizationFileStorage.GetImagesByInterviews();
            foreach (var image in images)
            {
                await this.synchronizationService.UploadInterviewImageAsync(
                    interviewId: image.InterviewId,
                    fileName: image.FileName,
                    fileData: image.GetData(),
                    onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                    token: synchronizationCancellationTokenSource.Token);

                this.interviewSynchronizationFileStorage.RemoveInterviewImage(image.InterviewId, image.FileName);
            }
        }

        private async Task DownloadQuestionnaireIfDoesNotExistsAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            if (this.questionnaireModelRepository.GetById(questionnaireIdentity.ToString()) == null)
            {
                var questionnaireApiView = await this.synchronizationService.GetQuestionnaireAsync(
                   questionnaire: questionnaireIdentity,
                   onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                   token: synchronizationCancellationTokenSource.Token);

                var questionnaireModel = this.questionnaireModelBuilder.BuildQuestionnaireModel(questionnaireApiView.Document);
                this.questionnaireModelRepository.Store(questionnaireModel, questionnaireIdentity.ToString());
                this.questionnaireRepository.StoreQuestionnaire(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, questionnaireApiView.Document);
                this.commandService.Execute(new RegisterPlainQuestionnaire(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, questionnaireApiView.AllowCensus, string.Empty));

                if (questionnaireApiView.AllowCensus)
                {
                    await this.plainStorageQuestionnireCensusInfo.StoreAsync(
                        new CensusQuestionnireInfo() { Id = questionnaireIdentity.ToString() }
                    );
                }
            }

            if (!this.questionnaireAssemblyFileAccessor.IsQuestionnaireAssemblyExists(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version))
            {
                var questionnaireAssembly = await this.synchronizationService.GetQuestionnaireAssemblyAsync(
                   questionnaire: questionnaireIdentity,
                   onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                   token: synchronizationCancellationTokenSource.Token);

                this.questionnaireAssemblyFileAccessor.StoreAssembly(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, questionnaireAssembly);   
            }
        }

        public void CancelSynchronizaion()
        {
            this.synchronizationCancellationTokenSource.Cancel();
        }
    }
}