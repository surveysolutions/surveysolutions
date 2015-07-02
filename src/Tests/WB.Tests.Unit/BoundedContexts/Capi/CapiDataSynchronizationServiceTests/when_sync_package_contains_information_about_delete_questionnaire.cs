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
    internal class when_sync_package_contains_information_about_delete_questionnaire: CapiDataSynchronizationServiceTestContext
    {
        Establish context = () =>
        {
            questionnaireMetadata = new QuestionnaireMetadata(Guid.NewGuid(), 1, false);

            syncItem = new QuestionnaireSyncPackageDto
                       {
                           Content = "some content", 
                           MetaInfo = "some metadata"
                       };

            var jsonUtilsMock = new Mock<IJsonUtils>();
            jsonUtilsMock.Setup(x => x.Deserialize<QuestionnaireMetadata>(syncItem.MetaInfo)).Returns(questionnaireMetadata);

            commandService = new Mock<ICommandService>();

            plainQuestionnaireRepositoryMock=new Mock<IPlainQuestionnaireRepository>();

            changeLogManipulator = new Mock<IChangeLogManipulator>();
            capiDataSynchronizationService = CreateCapiDataSynchronizationService(changeLogManipulator.Object, commandService.Object, jsonUtilsMock.Object, null,
                plainQuestionnaireRepositoryMock.Object);
        };

        Because of = () => capiDataSynchronizationService.ProcessDownloadedPackage(syncItem, SyncItemType.DeleteQuestionnaire);

        It should_call_DeleteQuestionnaire_once =
            () => commandService.Verify(x => x.Execute(
                            Moq.It.Is<DeleteQuestionnaire>(
                                param => param.QuestionnaireId == questionnaireMetadata.QuestionnaireId && param.QuestionnaireVersion == 1), null, false),
                    Times.Once);

        It should_delete_questionnaire_from_plaine_storage_once =
            () => plainQuestionnaireRepositoryMock.Verify(x => x.DeleteQuestionnaireDocument(questionnaireMetadata.QuestionnaireId, 1), Times.Once);

        private static CapiDataSynchronizationService capiDataSynchronizationService;
        private static QuestionnaireSyncPackageDto syncItem;
        private static Mock<ICommandService> commandService;
        private static Mock<IPlainQuestionnaireRepository> plainQuestionnaireRepositoryMock;
        private static Mock<IChangeLogManipulator> changeLogManipulator;
        private static QuestionnaireMetadata questionnaireMetadata;
    }
}
