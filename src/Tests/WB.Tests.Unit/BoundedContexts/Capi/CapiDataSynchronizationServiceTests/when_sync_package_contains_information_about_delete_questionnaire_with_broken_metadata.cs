using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Capi.ChangeLog;
using WB.Core.BoundedContexts.Capi.Implementation.Services;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Capi.CapiDataSynchronizationServiceTests
{
    internal class when_sync_package_contains_information_about_delete_questionnaire_with_broken_metadata : CapiDataSynchronizationServiceTestContext
    {
        Establish context = () =>
        {
            syncItem = new QuestionnaireSyncPackageDto
                       {
                           Content = "some content", 
                           MetaInfo = "some metadata"
                       };

            var jsonUtilsMock = new Mock<IJsonUtils>();
            jsonUtilsMock.Setup(x => x.Deserialize<QuestionnaireMetadata>(syncItem.MetaInfo)).Throws<NullReferenceException>();

            commandService = new Mock<ICommandService>();

            plainQuestionnaireRepositoryMock = new Mock<IPlainQuestionnaireRepository>();
            changeLogManipulator = new Mock<IChangeLogManipulator>();
            capiDataSynchronizationService = CreateCapiDataSynchronizationService(changeLogManipulator.Object, commandService.Object, jsonUtilsMock.Object, null,
                plainQuestionnaireRepositoryMock.Object);
        };

        Because of = () => exception = Catch.Exception(() => capiDataSynchronizationService.ProcessDownloadedPackage(syncItem, SyncItemType.DeleteQuestionnaire));

        It should_not_call_RegisterPlainQuestionnaire =
            () => commandService.Verify(x => x.Execute(Moq.It.Is<DeleteQuestionnaire>(param =>
                                    param.QuestionnaireId == Moq.It.IsAny<Guid>() && param.QuestionnaireVersion == Moq.It.IsAny<long>()), null),
                    Times.Never);

        It should_not_store_questionnaire_in_pline_storage =
            () => plainQuestionnaireRepositoryMock.Verify(x => x.DeleteQuestionnaireDocument(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>()), Times.Never);

        It should_throw_ArgumentException = () =>
            exception.ShouldBeOfType<ArgumentException>();

        private static CapiDataSynchronizationService capiDataSynchronizationService;
        private static QuestionnaireSyncPackageDto syncItem;
        private static Mock<ICommandService> commandService;
        private static Mock<IPlainQuestionnaireRepository> plainQuestionnaireRepositoryMock;
        private static Exception exception;
        private static Mock<IChangeLogManipulator> changeLogManipulator;
    }
}
