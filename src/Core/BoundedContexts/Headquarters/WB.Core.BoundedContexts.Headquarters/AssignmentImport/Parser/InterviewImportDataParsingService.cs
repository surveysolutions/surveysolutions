using System;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser
{
    internal class InterviewImportDataParsingService : IInterviewImportDataParsingService
    {
        private readonly IPreloadedDataServiceFactory preloadedDataServiceFactory;
        private readonly IPreloadedDataRepository preloadedDataRepository;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IQuestionnaireExportStructureStorage questionnaireExportStructureStorage;
        private readonly IPlainTransactionManagerProvider plainTransactionManagerProvider;
        private readonly IRosterStructureService rosterStructureService;

        public InterviewImportDataParsingService(
            IPreloadedDataServiceFactory preloadedDataServiceFactory, 
            IPreloadedDataRepository preloadedDataRepository, 
            IQuestionnaireStorage questionnaireStorage, IPlainTransactionManagerProvider plainTransactionManagerProvider,
            IQuestionnaireExportStructureStorage questionnaireExportStructureStorage,
            IRosterStructureService rosterStructureService)
        {
            this.preloadedDataServiceFactory = preloadedDataServiceFactory;
            this.preloadedDataRepository = preloadedDataRepository;
            this.questionnaireStorage = questionnaireStorage;
            this.plainTransactionManagerProvider = plainTransactionManagerProvider;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
            this.rosterStructureService = rosterStructureService;
        }

        public AssignmentImportData[] GetAssignmentsData(string interviewImportProcessId, QuestionnaireIdentity questionnaireIdentity, AssignmentImportType mode)
        {
            var preloadedDataService = this.GetPreloadedDataService(questionnaireIdentity);

            AssignmentPreloadedDataRecord[] dataToPreload = mode == AssignmentImportType.Panel 
                ? preloadedDataService.CreatePreloadedDataDtosFromPanelData(this.preloadedDataRepository.GetPreloadedDataOfPanel(interviewImportProcessId))
                : preloadedDataService.CreatePreloadedDataDtoFromAssignmentData(this.preloadedDataRepository.GetPreloadedDataOfSample(interviewImportProcessId));

            return dataToPreload?.Select(d => new AssignmentImportData
            {
                PreloadedData = d.PreloadedDataDto,
                InterviewerId = d.InterviewerId,
                SupervisorId = d.SupervisorId,
                Quantity = d.Quantity
            }).ToArray();
        }
        
        private IPreloadedDataService GetPreloadedDataService(QuestionnaireIdentity questionnaireIdentity)
        {
            var bigTemplateObject = this.plainTransactionManagerProvider.GetPlainTransactionManager().ExecuteInPlainTransaction(
                () => this.questionnaireStorage.GetQuestionnaireDocument(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version));

            if (bigTemplateObject == null)
                throw new Exception("Questionnaire was not found");

            var questionnaireExportStructure = this.questionnaireExportStructureStorage.GetQuestionnaireExportStructure(new QuestionnaireIdentity(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version));

            var questionnaireRosterStructure = this.rosterStructureService.GetRosterScopes(bigTemplateObject);

            var preloadedDataService = this.preloadedDataServiceFactory.CreatePreloadedDataService(
                questionnaireExportStructure, questionnaireRosterStructure, bigTemplateObject);
            return preloadedDataService;
        }


        private InterviewImportData[] GetInterviewsImport(string interviewImportProcessId, QuestionnaireIdentity questionnaireIdentity, bool isPanel)
        {
            var bigTemplateObject =
                this.plainTransactionManagerProvider.GetPlainTransactionManager().ExecuteInPlainTransaction(() => this.questionnaireStorage.GetQuestionnaireDocument(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version));

			if (bigTemplateObject == null)
                throw new Exception("Questionnaire was not found");
            var questionnaireExportStructure =
                this.questionnaireExportStructureStorage.GetQuestionnaireExportStructure(
                    new QuestionnaireIdentity(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version));

            var questionnaireRosterStructure = this.rosterStructureService.GetRosterScopes(bigTemplateObject);

            var preloadedDataService = this.preloadedDataServiceFactory.CreatePreloadedDataService(questionnaireExportStructure,
                    questionnaireRosterStructure, bigTemplateObject);

            PreloadedDataRecord[] dataToPreload = isPanel 
                ? preloadedDataService.CreatePreloadedDataDtosFromPanelData(this.preloadedDataRepository.GetPreloadedDataOfPanel(interviewImportProcessId)) 
                : preloadedDataService.CreatePreloadedDataDtoFromAssignmentData(this.preloadedDataRepository.GetPreloadedDataOfSample(interviewImportProcessId));

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