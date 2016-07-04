using System;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.SampleImport;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
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
            IPreloadedDataRepository preloadedDataRepository = null,
            IInterviewImportDataParsingService interviewImportDataParsingService=null,
            QuestionnaireDocument questionnaireDocument = null)
        {
            return new InterviewImportService(
                commandService: commandService ?? Mock.Of<ICommandService>(),
                logger: logger ?? Mock.Of<ILogger>(),
                sampleImportSettings: sampleImportSettings ?? Mock.Of<SampleImportSettings>(),
                preloadedDataRepository: preloadedDataRepository ?? Mock.Of<IPreloadedDataRepository>(),
                interviewImportDataParsingService:
                    interviewImportDataParsingService ?? Mock.Of<IInterviewImportDataParsingService>(),
                plainQuestionnaireRepository:
                    Mock.Of<IPlainQuestionnaireRepository>(
                        _ =>
                            _.GetQuestionnaireDocument(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>()) ==
                            questionnaireDocument),
                plainTransactionManagerProvider:
                    Mock.Of<IPlainTransactionManagerProvider>(
                        _ => _.GetPlainTransactionManager() == Mock.Of<IPlainTransactionManager>()));
        }
    }
}