using System;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;
using WB.Core.SharedKernels.SurveyManagement.Views.SampleImport;
using WB.Core.SharedKernels.SurveyManagement.Views.User;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services
{
    internal class SampleImportService : ISampleImportService
    {
        private readonly IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireDocumentVersionedStorage;
        private readonly IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureStorage;
        private readonly IReadSideKeyValueStorage<QuestionnaireRosterStructure> questionnaireRosterStructureStorage;
        private readonly ITemporaryDataStorage<SampleCreationStatus> tempSampleCreationStorage;
        private readonly IPreloadedDataServiceFactory preloadedDataServiceFactory;
        
        private static ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

        public SampleImportService(IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireDocumentVersionedStorage,
            ITemporaryDataStorage<SampleCreationStatus> tempSampleCreationStorage,
            IPreloadedDataServiceFactory preloadedDataServiceFactory,
            IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureStorage,
            IReadSideKeyValueStorage<QuestionnaireRosterStructure> questionnaireRosterStructureStorage)
        {
            this.questionnaireDocumentVersionedStorage = questionnaireDocumentVersionedStorage;
            this.tempSampleCreationStorage = tempSampleCreationStorage;
            this.preloadedDataServiceFactory = preloadedDataServiceFactory;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
            this.questionnaireRosterStructureStorage = questionnaireRosterStructureStorage;
        }

        public void CreatePanel(Guid questionnaireId, long version, string id, PreloadedDataByFile[] data, Guid responsibleHeadquarterId,
            Guid? responsibleSupervisorId)
        {
            this.tempSampleCreationStorage.Store(new SampleCreationStatus(id), id);

            this.CreateInterviewInternal(
                questionnaireId,
                version,
                id,
                preloadedDataService => preloadedDataService.CreatePreloadedDataDtosFromPanelData(data),
                responsibleHeadquarterId,
                responsibleSupervisorId);
        }

        public void CreateSample(Guid questionnaireId, long version, string id, PreloadedDataByFile data, Guid responsibleHeadquarterId,
            Guid? responsibleSupervisorId)
        {
            this.tempSampleCreationStorage.Store(new SampleCreationStatus(id), id);

            this.CreateInterviewInternal(
                questionnaireId, 
                version, 
                id, 
                preloadedDataService => preloadedDataService.CreatePreloadedDataDtoFromSampleData(data), 
                responsibleHeadquarterId, 
                responsibleSupervisorId);
        }

        public SampleCreationStatus GetSampleCreationStatus(string id)
        {
            return this.tempSampleCreationStorage.GetByName(id);
        }

        private void CreateInterviewInternal(Guid questionnaireId, 
            long version, 
            string id,
            Func<IPreloadedDataService, PreloadedDataRecord[]> preloadedDataDtoCreator,
            Guid responsibleHeadquarterId, 
            Guid? responsibleSupervisorId)
        {
            var result = this.GetSampleCreationStatus(id);

            var bigTemplateObject = this.questionnaireDocumentVersionedStorage.AsVersioned().Get(questionnaireId.FormatGuid(), version);
            var questionnaireExportStructure = this.questionnaireExportStructureStorage.AsVersioned().Get(questionnaireId.FormatGuid(), version);
            var questionnaireRosterStructure = this.questionnaireRosterStructureStorage.AsVersioned().Get(questionnaireId.FormatGuid(), version);
            var bigTemplate = bigTemplateObject == null ? null : bigTemplateObject.Questionnaire;

            if (bigTemplate == null || questionnaireExportStructure == null || questionnaireRosterStructure==null)
            {
                result.SetErrorMessage("Questionnaire is absent");
                this.tempSampleCreationStorage.Store(result, id);
                return;
            }

            var preloadedDataService = this.preloadedDataServiceFactory.CreatePreloadedDataService(questionnaireExportStructure,
                questionnaireRosterStructure, bigTemplateObject.Questionnaire);

            result.SetStatusMessage("Data parsing");
            this.tempSampleCreationStorage.Store(result, id);

            var interviewForCreate = preloadedDataDtoCreator(preloadedDataService);

            if (interviewForCreate == null)
            {
                result.SetErrorMessage("Data parsing error");
                this.tempSampleCreationStorage.Store(result, id);
                return;
            }

            int errorCountOccuredOnInterviewsCreaition = 0;
            bool interviewLimitReached = false;
            DateTime startTime = DateTime.Now;
            
            var commandInvoker = ServiceLocator.Current.GetInstance<ICommandService>();
            for (int interviewIndex = 0; interviewIndex < interviewForCreate.Length; interviewIndex++)
            {
                try
                {
                    Guid responsibleId = interviewForCreate[interviewIndex].SupervisorId.HasValue
                        ? interviewForCreate[interviewIndex].SupervisorId.Value
                        : responsibleSupervisorId.Value;

                    commandInvoker.Execute(new CreateInterviewWithPreloadedData(Guid.NewGuid(), responsibleHeadquarterId,
                        bigTemplate.PublicKey, version, 
                        interviewForCreate[interviewIndex].PreloadedDataDto,
                        DateTime.UtcNow,
                        responsibleId), handleInBatch: true);

                    result.SetStatusMessage(string.Format(
                        @"Processed {0} interview(s) out of {1}. Spent time: {2:d\.hh\:mm\:ss}. Total estimated time: {3:d\.hh\:mm\:ss}.",
                        interviewIndex,
                        interviewForCreate.Length,
                        DateTime.Now - startTime,
                        CalulateEstimatedTime(interviewIndex, interviewForCreate.Length, startTime, DateTime.Now)));

                    this.tempSampleCreationStorage.Store(result, id);
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message, e);

                    errorCountOccuredOnInterviewsCreaition ++;
                    var interviewException = e as InterviewException;
                    if (interviewException != null && interviewException.ExceptionType == InterviewDomainExceptionType.InterviewLimitReached)
                    {
                        interviewLimitReached = true;
                        break;
                    }
                }
            }

            if (errorCountOccuredOnInterviewsCreaition > 0)
            {
                if (interviewLimitReached)
                {
                    result.SetErrorMessage("You can't create more interviews because limit has been reached");
                }
                else
                {
                    result.SetErrorMessage(string.Format("Error{0} occurred during interview creation",
                        errorCountOccuredOnInterviewsCreaition == 1 ? "" : "s"));
                }
            }
            else
                result.CompleteProcess();

            this.tempSampleCreationStorage.Store(result, id);
        }

        private static TimeSpan CalulateEstimatedTime(int interviewIndex, int totalInterviews, DateTime startTime, DateTime currentTime)
        {
            double spentMilliseconds = (currentTime - startTime).TotalMilliseconds;
            double estimatedMilliseconds = spentMilliseconds / (interviewIndex + 1) * totalInterviews;

            return TimeSpan.FromMilliseconds(estimatedMilliseconds);
        }
    }
}
