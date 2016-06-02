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
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;
        private readonly IPlainKeyValueStorage<QuestionnaireRosterStructure> questionnaireRosterStructureStorage;

        private readonly IQuestionnaireExportStructureStorage questionnaireExportStructureStorage;
        private readonly IPlainTransactionManagerProvider plainTransactionManagerProvider;

        public InterviewImportDataParsingService(
            IPreloadedDataServiceFactory preloadedDataServiceFactory, 
            IPreloadedDataRepository preloadedDataRepository, 
            IPlainQuestionnaireRepository plainQuestionnaireRepository, IPlainTransactionManagerProvider plainTransactionManagerProvider,
            IPlainKeyValueStorage<QuestionnaireRosterStructure> questionnaireRosterStructureStorage, 
            IQuestionnaireExportStructureStorage questionnaireExportStructureStorage)
        {
            this.preloadedDataServiceFactory = preloadedDataServiceFactory;
            this.preloadedDataRepository = preloadedDataRepository;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.plainTransactionManagerProvider = plainTransactionManagerProvider;
            this.questionnaireRosterStructureStorage = questionnaireRosterStructureStorage;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
        }

        public InterviewImportData[] GetInterviewsImportData(string interviewImportProcessId, QuestionnaireIdentity questionnaireIdentity)
        {
            var bigTemplateObject =
                this.plainTransactionManagerProvider.GetPlainTransactionManager().ExecuteInPlainTransaction(() =>this.plainQuestionnaireRepository.GetQuestionnaireDocument(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version));

            var questionnaireExportStructure =
                this.questionnaireExportStructureStorage.GetQuestionnaireExportStructure(
                    new QuestionnaireIdentity(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version));
            var questionnaireRosterStructure =
                this.questionnaireRosterStructureStorage.GetById(
                    new QuestionnaireIdentity(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version).ToString());


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