using System;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.Synchronization.SyncStorage;
using WB.Tests.Unit.SharedKernels.SurveyManagement.SynchronizationDenormalizerTests;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Synchronization.SimpleSynchronizationDataStorageTests
{
    internal class when_marking_interview_for_deleting : InterviewSynchronizationDenormalizerTestsContext
    {
        Establish context = () =>
        {
            var summaryItem = new InterviewSummary();
            var interviewSummarys = Mock.Of<IReadSideRepositoryWriter<InterviewSummary>>(x => x.GetById(interviewId.FormatGuid()) == summaryItem);
            interviewPackageStorageWriter = new Mock<IOrderableSyncPackageWriter<InterviewSyncPackageMetaInformation>>();
            interviewSyncPackageContentStorage = new Mock<IReadSideKeyValueStorage<InterviewSyncPackageContent>>();

            denormalizer = CreateDenormalizer(
                interviewSummarys: interviewSummarys,
                interviewPackageStorageWriter: interviewPackageStorageWriter.Object,
                interviewSyncPackageContentStorage: interviewSyncPackageContentStorage.Object);
        };

        Because of = () => denormalizer.Handle(Create.InterviewHardDeletedEvent(userId: responsibleId.FormatGuid(), interviewId: interviewId));

        It should_store_chunck = () =>
          interviewPackageStorageWriter.Verify(x => x.Store(
              Moq.It.Is<InterviewSyncPackageMetaInformation>(s => s.ItemType == SyncItemType.DeleteInterview &&
                  s.InterviewId == interviewId), Moq.It.IsAny<string>()), Times.Once);

        It should_store_content_of_deletion_package = () =>
          interviewSyncPackageContentStorage.Verify(x =>
              x.Store(Moq.It.IsAny<InterviewSyncPackageContent>(), Moq.It.IsAny<string>()), Times.Once);

        private static InterviewSynchronizationDenormalizer denormalizer;

        private static Mock<IOrderableSyncPackageWriter<InterviewSyncPackageMetaInformation>> interviewPackageStorageWriter;
        private static Mock<IReadSideKeyValueStorage<InterviewSyncPackageContent>> interviewSyncPackageContentStorage;

        private static Guid responsibleId = Guid.Parse("1BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid interviewId = Guid.Parse("1BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBA");
    }
}
