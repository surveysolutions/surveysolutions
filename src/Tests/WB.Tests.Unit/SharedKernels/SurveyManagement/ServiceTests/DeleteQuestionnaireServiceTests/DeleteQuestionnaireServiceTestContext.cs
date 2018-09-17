using Moq;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.DeleteQuestionnaireTemplate;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.DeleteQuestionnaireTemplate;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.Questionnaire;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DeleteQuestionnaireServiceTests
{
    [NUnit.Framework.TestOf(typeof(DeleteQuestionnaireService))]
    internal class DeleteQuestionnaireServiceTestContext
    {
        protected static DeleteQuestionnaireService CreateDeleteQuestionnaireService(IInterviewsToDeleteFactory interviewsToDeleteFactory = null,
           ICommandService commandService = null, 
           IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage = null, 
           IQuestionnaireStorage questionnaireStorage = null,
           IAssignmentsImportService interviewImportService = null)
        {
            IInterviewsToDeleteFactory Factory() => interviewsToDeleteFactory ?? Mock.Of<IInterviewsToDeleteFactory>();

            Setup.InstanceToMockedServiceLocator(questionnaireBrowseItemStorage ?? Mock.Of<IPlainStorageAccessor<QuestionnaireBrowseItem>>());
            return
                new DeleteQuestionnaireService(
                    Factory,
                    commandService ?? Mock.Of<ICommandService>(), Mock.Of<ILogger>(),
                    Mock.Of<ITranslationManagementService>(),
                    interviewImportService ?? Mock.Of<IAssignmentsImportService>(_ => _.GetImportStatus() == new AssignmentsImportStatus()),
                    Mock.Of<IAuditLog>(),
                    Mock.Of<IPlainStorageAccessor<QuestionnaireBrowseItem>>(),
                    Mock.Of<IAssignmetnsDeletionService>(),
                    Mock.Of<IPlainKeyValueStorage<QuestionnaireLookupTable>>(),
                    Mock.Of<IQuestionnaireStorage>());
        }
    }
}
