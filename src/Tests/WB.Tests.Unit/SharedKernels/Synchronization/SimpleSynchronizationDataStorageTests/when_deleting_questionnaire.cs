using System;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.Synchronization.SyncStorage;
using WB.Tests.Unit.SharedKernels.SurveyManagement.SynchronizationDenormalizerTests;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Synchronization.SimpleSynchronizationDataStorageTests
{
    internal class when_deleting_questionnaire : QuestionnaireSynchronizationDenormalizerTestsContext
    {
        Establish context = () =>
        {
            questionnairePackageStorageWriter = new Mock<IOrderableSyncPackageWriter<QuestionnaireSyncPackageMetaInformation>>();
            questionnaireSyncPackageContentStorageMock=new Mock<IReadSideKeyValueStorage<QuestionnaireSyncPackageContent>>();
            denormalizer = CreateDenormalizer(questionnairePackageStorageWriter: questionnairePackageStorageWriter.Object, questionnaireSyncPackageContentStorage: questionnaireSyncPackageContentStorageMock.Object);
        };

        private Because of = () => denormalizer.Handle(Create.QuestionaireDeleted(questionnaireId, version));

        It should_store_delete_package_meta_information = () =>
            questionnairePackageStorageWriter.Verify(
                x => x.Store(Moq.It.Is<QuestionnaireSyncPackageMetaInformation>(s => s.ItemType == SyncItemType.DeleteQuestionnaire), Moq.It.IsAny<string>()),
                Times.Once);
        It should_store_delete_package_content = () =>
            questionnaireSyncPackageContentStorageMock.Verify(
             x => x.Store(Moq.It.IsAny<QuestionnaireSyncPackageContent>(), Moq.It.IsAny<string>()),
             Times.Once);

        private static QuestionnaireSynchronizationDenormalizer denormalizer;
        private static Guid questionnaireId = Guid.Parse("1BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static long version = 4;
        private static Mock<IOrderableSyncPackageWriter<QuestionnaireSyncPackageMetaInformation>> questionnairePackageStorageWriter;
        private static Mock<IReadSideKeyValueStorage<QuestionnaireSyncPackageContent>> questionnaireSyncPackageContentStorageMock;
    }
}
