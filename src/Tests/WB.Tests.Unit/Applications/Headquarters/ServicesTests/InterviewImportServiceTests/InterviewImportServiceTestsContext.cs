using System;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.SampleImport;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Tests.Abc;
using WB.UI.Headquarters.Implementation.Services;

namespace WB.Tests.Unit.Applications.Headquarters.ServicesTests.InterviewImportServiceTests
{
    public class InterviewImportServiceTestsContext
    {
        public static InterviewImportService CreateInterviewImportService(
            ICommandService commandService = null,
            ITransactionManagerProvider transactionManager = null,
            ILogger logger = null,
            SampleImportSettings sampleImportSettings = null,
            IInterviewImportDataParsingService interviewImportDataParsingService=null,
            QuestionnaireDocument questionnaireDocument = null,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory = null,
            IInterviewTreeBuilder interviewTreeBuilder = null,
            IPreloadedDataRepository preloadedDataRepository = null,
            IPreloadedDataVerifier preloadedDataVerifier = null)
        {
            var plainQuestionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument, 1);
            var questionnaireStorage = Mock.Of<IQuestionnaireStorage>(_ 
                => _.GetQuestionnaireDocument(Moq.It.IsAny<Guid>(), It.IsAny<long>()) == questionnaireDocument
                && _.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), null) == plainQuestionnaire);

            return new InterviewImportService(
                commandService: commandService ?? Mock.Of<ICommandService>(),
                logger: logger ?? Mock.Of<ILogger>(),
                sampleImportSettings: sampleImportSettings ?? Mock.Of<SampleImportSettings>(),
                interviewImportDataParsingService:
                    interviewImportDataParsingService ?? Mock.Of<IInterviewImportDataParsingService>(),
                questionnaireStorage: questionnaireStorage,
                interviewKeyGenerator: Mock.Of<IInterviewUniqueKeyGenerator>(),
                plainTransactionManagerProvider:
                    Mock.Of<IPlainTransactionManagerProvider>(
                        _ => _.GetPlainTransactionManager() == Mock.Of<IPlainTransactionManager>()),
                transactionManagerProvider: Create.Service.TransactionManagerProvider(),
                assignmentPlainStorageAccessor: Mock.Of<IPlainStorageAccessor<Assignment>>(),
                userViewFactory : Mock.Of<IUserViewFactory>(),
                interviewTreeBuilder: interviewTreeBuilder ?? Mock.Of<IInterviewTreeBuilder>(),
                preloadedDataRepository: preloadedDataRepository ?? Mock.Of<IPreloadedDataRepository>(),
                preloadedDataVerifier: preloadedDataVerifier ?? Mock.Of<IPreloadedDataVerifier>());
        }
    }
}