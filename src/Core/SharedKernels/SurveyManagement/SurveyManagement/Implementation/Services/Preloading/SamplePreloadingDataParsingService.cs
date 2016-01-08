using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading
{
    internal class SamplePreloadingDataParsingService : ISamplePreloadingDataParsingService
    {
        private readonly IPreloadedDataServiceFactory preloadedDataServiceFactory;
        private readonly IPreloadedDataRepository preloadedDataRepository;
        private readonly IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireDocumentRepository;
        private readonly IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureStorage;
        private readonly IReadSideKeyValueStorage<QuestionnaireRosterStructure> questionnaireRosterStructureStorage;
        private readonly ITransactionManagerProvider transactionManager;

        public SamplePreloadingDataParsingService(
            IPreloadedDataServiceFactory preloadedDataServiceFactory, 
            IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireDocumentRepository, 
            IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureStorage, 
            IReadSideKeyValueStorage<QuestionnaireRosterStructure> questionnaireRosterStructureStorage, 
            ITransactionManagerProvider transactionManager, IPreloadedDataRepository preloadedDataRepository)
        {
            this.preloadedDataServiceFactory = preloadedDataServiceFactory;
            this.questionnaireDocumentRepository = questionnaireDocumentRepository;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
            this.questionnaireRosterStructureStorage = questionnaireRosterStructureStorage;
            this.transactionManager = transactionManager;
            this.preloadedDataRepository = preloadedDataRepository;
        }

        public InterviewSampleData[] ParseSample(string sampleId, QuestionnaireIdentity questionnaireIdentity)
        {
            var bigTemplateObject =
                this.transactionManager.GetTransactionManager()
                    .ExecuteInQueryTransaction(() => this.questionnaireDocumentRepository.AsVersioned()
                        .Get(questionnaireIdentity.QuestionnaireId.FormatGuid(), questionnaireIdentity.Version));

            var questionnaireExportStructure =
                this.transactionManager.GetTransactionManager()
                    .ExecuteInQueryTransaction(() => this.questionnaireExportStructureStorage.AsVersioned()
                        .Get(questionnaireIdentity.QuestionnaireId.FormatGuid(), questionnaireIdentity.Version));
            var questionnaireRosterStructure =
                this.transactionManager.GetTransactionManager()
                    .ExecuteInQueryTransaction(() => this.questionnaireRosterStructureStorage.AsVersioned()
                        .Get(questionnaireIdentity.QuestionnaireId.FormatGuid(), questionnaireIdentity.Version));


            var preloadedDataService =
                this.preloadedDataServiceFactory.CreatePreloadedDataService(questionnaireExportStructure,
                    questionnaireRosterStructure, bigTemplateObject.Questionnaire);

            var preloadedDataOfSample = this.preloadedDataRepository.GetPreloadedDataOfSample(sampleId);

            var dataToPreload = preloadedDataService.CreatePreloadedDataDtoFromSampleData(preloadedDataOfSample);

            return
                dataToPreload.Select(
                    d =>
                        new InterviewSampleData()
                        {
                            Answers = d.PreloadedDataDto.Data[0].Answers,
                            InterviewerId = d.InterviewerId,
                            SupervisorId = d.SupervisorId
                        })
                    .ToArray();
        }
    }
}