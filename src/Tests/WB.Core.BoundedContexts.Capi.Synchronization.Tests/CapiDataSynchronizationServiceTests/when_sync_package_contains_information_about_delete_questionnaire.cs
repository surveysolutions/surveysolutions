using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Capi.Synchronization.ChangeLog;
using WB.Core.BoundedContexts.Capi.Synchronization.Implementation.Services;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContext.Capi.Synchronization.Tests.CapiDataSynchronizationServiceTests
{
    internal class when_sync_package_contains_information_about_delete_questionnaire: CapiDataSynchronizationServiceTestContext
    {
        Establish context = () =>
        {

            questionnaireMetadata = new QuestionnaireMetadata(Guid.NewGuid(), 1, false);

            syncItem = new SyncItem() { ItemType = SyncItemType.DeleteTemplate, IsCompressed = true, Content = "some content", MetaInfo = "some metadata", Id = Guid.NewGuid() };

            var jsonUtilsMock = new Mock<IJsonUtils>();
            jsonUtilsMock.Setup(x => x.Deserrialize<QuestionnaireMetadata>(syncItem.MetaInfo)).Returns(questionnaireMetadata);

            commandService = new Mock<ICommandService>();

            plainQuestionnaireRepositoryMock=new Mock<IPlainQuestionnaireRepository>();

            changeLogManipulator = new Mock<IChangeLogManipulator>();
            capiDataSynchronizationService = CreateCapiDataSynchronizationService(changeLogManipulator.Object, commandService.Object, jsonUtilsMock.Object, null,
                plainQuestionnaireRepositoryMock.Object);
        };

        Because of = () => capiDataSynchronizationService.SavePulledItem(syncItem);

        It should_call_DeleteQuestionnaire_once =
            () =>
                commandService.Verify(
                    x =>
                        x.Execute(
                            Moq.It.Is<DeleteQuestionnaire>(
                                param =>
                                    param.QuestionnaireId == questionnaireMetadata.QuestionnaireId && param.QuestionnaireVersion == 1), null),
                    Times.Once);

        It should_delete_questionnaire_from_plaine_storage_once =
            () =>
                plainQuestionnaireRepositoryMock.Verify(
                    x => x.DeleteQuestionnaireDocument(questionnaireMetadata.QuestionnaireId, 1),
                    Times.Once);

        It should_create_public_record_in_change_log_for_sync_item_once =
        () =>
            changeLogManipulator.Verify(
                x =>
                    x.CreatePublicRecord(syncItem.Id),
                Times.Once);

        private static CapiDataSynchronizationService capiDataSynchronizationService;
        private static SyncItem syncItem;
        private static Mock<ICommandService> commandService;
        private static Mock<IPlainQuestionnaireRepository> plainQuestionnaireRepositoryMock;
        private static Mock<IChangeLogManipulator> changeLogManipulator;
        private static QuestionnaireMetadata questionnaireMetadata;
    }
}
