using System;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;
using WB.Core.SharedKernels.SurveyManagement.Views.SampleImport;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services
{
    internal class SampleImportService : ISampleImportService
    {
        private readonly IVersionedReadSideRepositoryReader<QuestionnaireDocumentVersioned> questionnaireDocumentVersionedStorage;
        private readonly IVersionedReadSideRepositoryReader<QuestionnaireExportStructure> questionnaireExportStructureStorage;
        private readonly IVersionedReadSideRepositoryReader<QuestionnaireRosterStructure> questionnaireRosterStructureStorage;
        private readonly ITemporaryDataStorage<SampleCreationStatus> tempSampleCreationStorage;
        private readonly IPreloadedDataServiceFactory preloadedDataServiceFactory;
        private static ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

        public SampleImportService(IVersionedReadSideRepositoryReader<QuestionnaireDocumentVersioned> questionnaireDocumentVersionedStorage,
            ITemporaryDataStorage<SampleCreationStatus> tempSampleCreationStorage,
            IPreloadedDataServiceFactory preloadedDataServiceFactory,
            IVersionedReadSideRepositoryReader<QuestionnaireExportStructure> questionnaireExportStructureStorage,
            IVersionedReadSideRepositoryReader<QuestionnaireRosterStructure> questionnaireRosterStructureStorage)
        {
            this.questionnaireDocumentVersionedStorage = questionnaireDocumentVersionedStorage;
            this.tempSampleCreationStorage = tempSampleCreationStorage;
            this.preloadedDataServiceFactory = preloadedDataServiceFactory;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
            this.questionnaireRosterStructureStorage = questionnaireRosterStructureStorage;
        }

        public void CreatePanel(Guid questionnaireId, long version, string id, PreloadedDataByFile[] data, Guid responsibleHeadquarterId,
            Guid responsibleSupervisorId)
        {
            this.tempSampleCreationStorage.Store(new SampleCreationStatus(id), id);
            new Task(() => this.CreateInterviewInternal(questionnaireId, version, id, (preloadedDataService) => preloadedDataService.CreatePreloadedDataDtosFromPanelData(data), responsibleHeadquarterId, responsibleSupervisorId))
                .Start();
        }

        public void CreateSample(Guid questionnaireId, long version, string id, PreloadedDataByFile data, Guid responsibleHeadquarterId,
            Guid responsibleSupervisorId)
        {

            this.tempSampleCreationStorage.Store(new SampleCreationStatus(id), id);
            new Task(() => this.CreateInterviewInternal(questionnaireId, 
                    version, 
                    id, 
                    preloadedDataService => preloadedDataService.CreatePreloadedDataDtoFromSampleData(data), 
                    responsibleHeadquarterId, 
                    responsibleSupervisorId))
                .Start();
        }

        public SampleCreationStatus GetSampleCreationStatus(string id)
        {
            return this.tempSampleCreationStorage.GetByName(id);
        }

        private void CreateInterviewInternal(Guid questionnaireId, 
            long version, 
            string id, 
            Func<IPreloadedDataService, PreloadedDataDto[]> preloadedDataDtoCreator,
            Guid responsibleHeadquarterId, 
            Guid responsibleSupervisorId)
        {
            var result = this.GetSampleCreationStatus(id);

            var bigTemplateObject = this.questionnaireDocumentVersionedStorage.GetById(questionnaireId, version);
            var questionnaireExportStructure = this.questionnaireExportStructureStorage.GetById(questionnaireId, version);
            var questionnaireRosterStructure = this.questionnaireRosterStructureStorage.GetById(questionnaireId, version);
            var bigTemplate = bigTemplateObject == null ? null : bigTemplateObject.Questionnaire;

            if (bigTemplate == null || questionnaireExportStructure == null || questionnaireRosterStructure==null)
            {
                result.SetErrorMessage("Template is absent");
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
                result.SetErrorMessage("Data pasing error");
                this.tempSampleCreationStorage.Store(result, id);
                return;
            }

            int errorCountOccuredOnInterviewsCreaition = 0;

            var commandInvoker = NcqrsEnvironment.Get<ICommandService>();
            for (int i = 0; i < interviewForCreate.Length; i++)
            {
                try
                {
                    commandInvoker.Execute(new CreateInterviewWithPreloadedData(Guid.NewGuid(), responsibleHeadquarterId,
                        bigTemplate.PublicKey, version, interviewForCreate[i],
                        DateTime.UtcNow, responsibleSupervisorId));

                    result.SetStatusMessage(string.Format("Processed {0} interview(s) from {1}", i, interviewForCreate.Length));
                    this.tempSampleCreationStorage.Store(result, id);
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message, e);

                    errorCountOccuredOnInterviewsCreaition ++;
                }
            }

            if (errorCountOccuredOnInterviewsCreaition > 0)
            {
                result.SetErrorMessage(string.Format("Error{0} occured during interview creation", errorCountOccuredOnInterviewsCreaition == 1 ? "" : "s"));
            }
            else
                result.CompleteProcess();

            this.tempSampleCreationStorage.Store(result, id);
        }
    }
}
