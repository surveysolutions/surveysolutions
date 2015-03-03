using Machine.Specifications;

using Moq;

using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.RepositoryAccessors;
using WB.Core.Synchronization.SyncStorage;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.OrderableSyncPackageWriterTests
{
    internal class when_storing_questionnaire_packages : OrderableSyncPackageWriterTestContext<QuestionnaireSyncPackageMeta, QuestionnaireSyncPackageContent>
    {
        Establish context = () =>
        {
            expectedPackageId = string.Format("{0}${1}", partialPackageId, storedIndex);
            meta = CreateQuestionnaireSyncPackageMeta();
            content = CreateQuestionnaireSyncPackageContent();
            var storedCounter = CreateSynchronizationDeltasCounter(storedIndex);

            packageMetaWriterMock = new Mock<IReadSideRepositoryWriter<QuestionnaireSyncPackageMeta>>();
            packageContentWriterMock = new Mock<IReadSideKeyValueStorage<QuestionnaireSyncPackageContent>>();
            counterStorageMock = new Mock<IReadSideKeyValueStorage<SynchronizationDeltasCounter>>();

            counterStorageMock.Setup(x => x.GetById(counterId)).Returns(storedCounter);

            writer = CreateOrderableSyncPackageWriter(
                counterStorage: counterStorageMock.Object,
                packageMetaWriter: packageMetaWriterMock.Object,
                packageContentWriter: packageContentWriterMock.Object);
        };

        Because of = () =>
            writer.Store(content, meta, partialPackageId, counterId);

        It should_stored_package_meta_with_updated_SortIndex_and_PackageId_fields = () =>
            packageMetaWriterMock.Verify(x => x.Store(
                Moq.It.Is<QuestionnaireSyncPackageMeta>(m => m.SortIndex == storedIndex && m.PackageId == expectedPackageId),
                expectedPackageId),
                Times.Once);

        It should_stored_package_content_with_updated_PackageId_field = () =>
            packageContentWriterMock.Verify(x => x.Store(
                Moq.It.Is<QuestionnaireSyncPackageContent>(m => m.PackageId == expectedPackageId),
                expectedPackageId),
                Times.Once);

        It should_get_stored_index_once = () =>
            counterStorageMock.Verify(x => x.GetById(counterId), Times.Once);

        It should_store_next_index_once = () =>
            counterStorageMock.Verify(x =>
                x.Store(Moq.It.Is<SynchronizationDeltasCounter>(c => c.CountOfStoredDeltas == storedIndex + 1), counterId), Times.Once);

        private static QuestionnaireSyncPackageMeta meta;
        private static string partialPackageId = "hello";
        private static QuestionnaireSyncPackageContent content;
        private const int storedIndex = 5;
        private static string expectedPackageId;
        private static Mock<IReadSideRepositoryWriter<QuestionnaireSyncPackageMeta>> packageMetaWriterMock;
        private static Mock<IReadSideKeyValueStorage<QuestionnaireSyncPackageContent>> packageContentWriterMock;
        private static Mock<IReadSideKeyValueStorage<SynchronizationDeltasCounter>> counterStorageMock;
        private static OrderableSyncPackageWriter<QuestionnaireSyncPackageMeta, QuestionnaireSyncPackageContent> writer;
        private static string counterId = "QuestionnaireCounter";
    }
}