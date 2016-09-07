using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

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

        public InterviewImportData[] GetInterviewsImportData(string interviewImportProcessId, QuestionnaireIdentity questionnaireIdentity)
        {
            var bigTemplateObject =
                this.plainTransactionManagerProvider.GetPlainTransactionManager().ExecuteInPlainTransaction(() =>this.questionnaireStorage.GetQuestionnaireDocument(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version));

            var questionnaireExportStructure =
                this.questionnaireExportStructureStorage.GetQuestionnaireExportStructure(
                    new QuestionnaireIdentity(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version));
            var questionnaireRosterStructure =
                this.questionnaireRosterStructureStorage.GetQuestionnaireRosterStructure(
                    new QuestionnaireIdentity(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version));

            var preloadedDataService =
                this.preloadedDataServiceFactory.CreatePreloadedDataService(questionnaireExportStructure,
                    questionnaireRosterStructure, bigTemplateObject);

            var preloadedDataOfSample = this.preloadedDataRepository.GetPreloadedDataOfSample(interviewImportProcessId);

            var dataToPreload = preloadedDataService.CreatePreloadedDataDtoFromSampleData(preloadedDataOfSample);

            return
                dataToPreload.Select(
                    d =>
                        new InterviewImportData()
                        {
                            Answers = d.PreloadedDataDto.Data[0].Answers,
                            InterviewerId = d.InterviewerId,
                            SupervisorId = d.SupervisorId
                        })
                    .ToArray();
        }
    }
}