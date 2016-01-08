using Moq;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.SampleImport;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.UI.Headquarters.Implementation.Services;

namespace WB.Tests.Unit.Applications.Headquarters.ServicesTests.InterviewImportServiceTests
{
    public class InterviewImportServiceTestsContext
    {
        public static InterviewImportService CreateInterviewImportService(
            IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireDocumentRepository = null,
            ICommandService commandService = null,
            ITransactionManagerProvider transactionManager = null,
            ILogger logger = null,
            SampleImportSettings sampleImportSettings = null,
            IPreloadedDataRepository preloadedDataRepository = null,
            IInterviewImportDataParsingService interviewImportDataParsingService=null)
        {
            if (transactionManager == null)
            {
                var mockOfTransactionManager = new Mock<ITransactionManagerProvider>();
                mockOfTransactionManager.Setup(x => x.GetTransactionManager()).Returns(Mock.Of<ITransactionManager>());
                transactionManager = mockOfTransactionManager.Object;
            }
            return new InterviewImportService(
                questionnaireDocumentRepository: questionnaireDocumentRepository ?? Mock.Of<IReadSideKeyValueStorage<QuestionnaireDocumentVersioned>>(),
                commandService: commandService ?? Mock.Of<ICommandService>(),
                transactionManager: transactionManager,
                logger: logger ?? Mock.Of<ILogger>(),
                sampleImportSettings: sampleImportSettings ?? Mock.Of<SampleImportSettings>(),
                preloadedDataRepository: preloadedDataRepository ?? Mock.Of<IPreloadedDataRepository>(),
                interviewImportDataParsingService: interviewImportDataParsingService ?? Mock.Of<IInterviewImportDataParsingService>());
        }
    }
}