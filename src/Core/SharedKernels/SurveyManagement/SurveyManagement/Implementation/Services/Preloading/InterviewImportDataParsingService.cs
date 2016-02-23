using System.Linq;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading
{
    internal class InterviewImportDataParsingService : IInterviewImportDataParsingService
    {
        private readonly IPreloadedDataServiceFactory preloadedDataServiceFactory;
        private readonly IPreloadedDataRepository preloadedDataRepository;
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;
        private readonly IQuestionnaireProjectionsRepository questionnaireProjectionsRepository;
        private readonly IPlainTransactionManager plainTransactionManager;

        public InterviewImportDataParsingService(
            IPreloadedDataServiceFactory preloadedDataServiceFactory, 
            IPreloadedDataRepository preloadedDataRepository, 
            IPlainQuestionnaireRepository plainQuestionnaireRepository, IPlainTransactionManager plainTransactionManager, 
            IQuestionnaireProjectionsRepository questionnaireProjectionsRepository)
        {
            this.preloadedDataServiceFactory = preloadedDataServiceFactory;
            this.preloadedDataRepository = preloadedDataRepository;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.plainTransactionManager = plainTransactionManager;
            this.questionnaireProjectionsRepository = questionnaireProjectionsRepository;
        }

        public InterviewImportData[] GetInterviewsImportData(string interviewImportProcessId, QuestionnaireIdentity questionnaireIdentity)
        {
            var bigTemplateObject =
                this.plainTransactionManager.ExecuteInPlainTransaction(() =>this.plainQuestionnaireRepository.GetQuestionnaireDocument(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version));

            var questionnaireExportStructure =
                this.questionnaireProjectionsRepository.GetQuestionnaireExportStructure(
                    new QuestionnaireIdentity(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version));
            var questionnaireRosterStructure =
                this.questionnaireProjectionsRepository.GetQuestionnaireRosterStructure(
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