using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services.Preloading
{
    internal class InterviewImportDataParsingService : IInterviewImportDataParsingService
    {
        private readonly IPreloadedDataServiceFactory preloadedDataServiceFactory;
        private readonly IPreloadedDataRepository preloadedDataRepository;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IQuestionnaireRosterStructureStorage questionnaireRosterStructureStorage;

        private readonly IQuestionnaireExportStructureStorage questionnaireExportStructureStorage;
        private readonly IPlainTransactionManagerProvider plainTransactionManagerProvider;

        public InterviewImportDataParsingService(
            IPreloadedDataServiceFactory preloadedDataServiceFactory, 
            IPreloadedDataRepository preloadedDataRepository, 
            IQuestionnaireStorage questionnaireStorage, IPlainTransactionManagerProvider plainTransactionManagerProvider,
            IQuestionnaireRosterStructureStorage questionnaireRosterStructureStorage, 
            IQuestionnaireExportStructureStorage questionnaireExportStructureStorage)
        {
            this.preloadedDataServiceFactory = preloadedDataServiceFactory;
            this.preloadedDataRepository = preloadedDataRepository;
            this.questionnaireStorage = questionnaireStorage;
            this.plainTransactionManagerProvider = plainTransactionManagerProvider;
            this.questionnaireRosterStructureStorage = questionnaireRosterStructureStorage;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
        }

        public InterviewImportData[] GetInterviewsImportDataForSample(string interviewImportProcessId, QuestionnaireIdentity questionnaireIdentity)
        {
            return this.GetInterviewsImport(interviewImportProcessId, questionnaireIdentity, false);
        }

        public InterviewImportData[] GetInterviewsImportDataForPanel(string interviewImportProcessId, QuestionnaireIdentity questionnaireIdentity)
        {
            return this.GetInterviewsImport(interviewImportProcessId, questionnaireIdentity, true);
        }

        private InterviewImportData[] GetInterviewsImport(string interviewImportProcessId, QuestionnaireIdentity questionnaireIdentity, bool isPanel)
        {
            var bigTemplateObject =
                this.plainTransactionManagerProvider.GetPlainTransactionManager().ExecuteInPlainTransaction(() => this.questionnaireStorage.GetQuestionnaireDocument(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version));

            var questionnaireExportStructure =
                this.questionnaireExportStructureStorage.GetQuestionnaireExportStructure(
                    new QuestionnaireIdentity(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version));
            var questionnaireRosterStructure =
                this.questionnaireRosterStructureStorage.GetQuestionnaireRosterStructure(
                    new QuestionnaireIdentity(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version));

            var preloadedDataService = this.preloadedDataServiceFactory.CreatePreloadedDataService(questionnaireExportStructure,
                    questionnaireRosterStructure, bigTemplateObject);

            PreloadedDataRecord[] dataToPreload = isPanel 
                ? preloadedDataService.CreatePreloadedDataDtosFromPanelData(this.preloadedDataRepository.GetPreloadedDataOfPanel(interviewImportProcessId)) 
                : preloadedDataService.CreatePreloadedDataDtoFromSampleData(this.preloadedDataRepository.GetPreloadedDataOfSample(interviewImportProcessId));

            return
                dataToPreload.Select(
                    d => new InterviewImportData()
                    {
                        PreloadedData = d.PreloadedDataDto,
                        InterviewerId = d.InterviewerId,
                        SupervisorId = d.SupervisorId
                    }).ToArray();
        }
    }
}