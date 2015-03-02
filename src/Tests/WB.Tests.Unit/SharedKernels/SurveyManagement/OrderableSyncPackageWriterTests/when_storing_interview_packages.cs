using Machine.Specifications;

using Moq;

using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.RepositoryAccessors;
using WB.Core.Synchronization.SyncStorage;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.OrderableSyncPackageWriterTests
{
    internal class when_storing_interview_packages : OrderableSyncPackageWriterTestContext<InterviewSyncPackageMeta, InterviewSyncPackageContent> 
    {
        Establish context = () =>
        {
            expectedPackageId = string.Format("{0}${1}", partialPackageId, storedIndex);
            meta = CreateInterviewSyncPackageMeta();
            content = CreateInterviewSyncPackageContent();
            var storedCounter = CreateSynchronizationDeltasCounter(storedIndex);

            packageMetaWriterMock = new Mock<IReadSideRepositoryWriter<InterviewSyncPackageMeta>>();
            packageContentWriterMock = new Mock<IReadSideKeyValueStorage<InterviewSyncPackageContent>>();
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
                Moq.It.Is<InterviewSyncPackageMeta>(m => m.SortIndex == storedIndex && m.PackageId == expectedPackageId),
                expectedPackageId),
                Times.Once);

        It should_stored_package_content_with_updated_PackageId_field = () =>
            packageContentWriterMock.Verify(x => x.Store(
                Moq.It.Is<InterviewSyncPackageContent>(m => m.PackageId == expectedPackageId),
                expectedPackageId),
                Times.Once);

        It should_get_stored_index_once = () =>
            counterStorageMock.Verify(x => x.GetById(counterId), Times.Once);

        It should_store_next_index_once = () =>
            counterStorageMock.Verify(x =>
                x.Store(Moq.It.Is<SynchronizationDeltasCounter>(c => c.CountOfStoredDeltas == storedIndex + 1), counterId), Times.Once);

        private static InterviewSyncPackageMeta meta;
        private static string partialPackageId = "hello";
        private static InterviewSyncPackageContent content;
        private const int storedIndex = 5;
        private static string expectedPackageId;
        private static Mock<IReadSideRepositoryWriter<InterviewSyncPackageMeta>> packageMetaWriterMock;
        private static Mock<IReadSideKeyValueStorage<InterviewSyncPackageContent>> packageContentWriterMock;
        private static Mock<IReadSideKeyValueStorage<SynchronizationDeltasCounter>> counterStorageMock;
        private static OrderableSyncPackageWriter<InterviewSyncPackageMeta, InterviewSyncPackageContent> writer;
        private static string counterId = "InterviewCounter";
    }
}