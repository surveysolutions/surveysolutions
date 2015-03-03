using Machine.Specifications;

using Moq;

using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.RepositoryAccessors;
using WB.Core.Synchronization.SyncStorage;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.OrderableSyncPackageWriterTests
{
    internal class when_storing_user_packages : OrderableSyncPackageWriterTestContext<UserSyncPackageMeta, UserSyncPackageContent>
    {
        Establish context = () =>
        {
            expectedPackageId = string.Format("{0}${1}", partialPackageId, storedIndex);
            meta = CreateUserSyncPackageMeta();
            content = CreateUserSyncPackageContent();
            var storedCounter = CreateSynchronizationDeltasCounter(storedIndex);

            packageMetaWriterMock = new Mock<IReadSideRepositoryWriter<UserSyncPackageMeta>>();
            packageContentWriterMock = new Mock<IReadSideKeyValueStorage<UserSyncPackageContent>>();
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
                Moq.It.Is<UserSyncPackageMeta>(m => m.SortIndex == storedIndex && m.PackageId == expectedPackageId),
                expectedPackageId),
                Times.Once);

        It should_stored_package_content_with_updated_PackageId_field = () =>
            packageContentWriterMock.Verify(x => x.Store(
                Moq.It.Is<UserSyncPackageContent>(m => m.PackageId == expectedPackageId),
                expectedPackageId),
                Times.Once);

        It should_get_stored_index_once = () =>
            counterStorageMock.Verify(x => x.GetById(counterId), Times.Once);

        It should_store_next_index_once = () =>
            counterStorageMock.Verify(x =>
                x.Store(Moq.It.Is<SynchronizationDeltasCounter>(c => c.CountOfStoredDeltas == storedIndex + 1), counterId), Times.Once);

        private static UserSyncPackageMeta meta;
        private static string partialPackageId = "hello";
        private static UserSyncPackageContent content;
        private const int storedIndex = 5;
        private static string expectedPackageId;
        private static Mock<IReadSideRepositoryWriter<UserSyncPackageMeta>> packageMetaWriterMock;
        private static Mock<IReadSideKeyValueStorage<UserSyncPackageContent>> packageContentWriterMock;
        private static Mock<IReadSideKeyValueStorage<SynchronizationDeltasCounter>> counterStorageMock;
        private static OrderableSyncPackageWriter<UserSyncPackageMeta, UserSyncPackageContent> writer;
        private static string counterId = "UserCounter";
    }
}