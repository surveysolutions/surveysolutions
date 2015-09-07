using System;
using System.Threading;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Services;

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
        private readonly CancellationTokenSource synchronizationCancellationTokenSource = new CancellationTokenSource();

        public SynchronizationViewModel(
            ISynchronizationService synchronizationService,
            IViewModelNavigationService viewModelNavigationService,
            IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor,
            IQuestionnaireModelBuilder questionnaireModelBuilder,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireModelRepository,
            IPlainQuestionnaireRepository questionnaireRepository,
            ICommandService commandService)
        {
            this.synchronizationService = synchronizationService;
            this.viewModelNavigationService = viewModelNavigationService;
            this.questionnaireAssemblyFileAccessor = questionnaireAssemblyFileAccessor;
            this.questionnaireModelBuilder = questionnaireModelBuilder;
            this.questionnaireModelRepository = questionnaireModelRepository;
            this.questionnaireRepository = questionnaireRepository;
            this.commandService = commandService;
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

                await this.DownloadCensusQuestionnairesAsync();
                await this.DownloadInterviewsAsync();
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
            }
        }

        private async Task DownloadQuestionnaireIfDoesNotExistsAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            if (this.questionnaireModelRepository.GetById(questionnaireIdentity.ToString()) == null)
            {
                var questionnaire = await this.synchronizationService.GetQuestionnaireAsync(
                   questionnaire: questionnaireIdentity,
                   onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                   token: synchronizationCancellationTokenSource.Token);
                var questionnaireModel = this.questionnaireModelBuilder.BuildQuestionnaireModel(questionnaire.Document);
                this.questionnaireModelRepository.Store(questionnaireModel, questionnaireIdentity.ToString());
                this.questionnaireRepository.StoreQuestionnaire(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, questionnaire.Document);
                this.commandService.Execute(new RegisterPlainQuestionnaire(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, questionnaire.AllowCensus, string.Empty));   
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