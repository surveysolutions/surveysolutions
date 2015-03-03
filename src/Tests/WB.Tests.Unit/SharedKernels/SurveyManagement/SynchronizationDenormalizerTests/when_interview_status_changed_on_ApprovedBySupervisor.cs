using System;

using Machine.Specifications;

using Moq;

using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.Synchronization.SyncStorage;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SynchronizationDenormalizerTests
{
    internal class when_interview_status_changed_on_ApprovedBySupervisor : InterviewSynchronizationDenormalizerTestsContext
    {
        Establish context = () =>
        {
            denormalizer = CreateDenormalizer(interviewPackageStorageWriter: interviewPackageStorageWriterMock.Object);
        };

        Because of = () =>
            denormalizer.Handle(Create.Event.InterviewStatusChanged(interviewId, InterviewStatus.ApprovedBySupervisor));

        It should_not_store_any_sync_package = () =>
            interviewPackageStorageWriterMock.Verify(
                x => x.Store(
                    Moq.It.IsAny<InterviewSyncPackageContent>(),
                    Moq.It.IsAny<InterviewSyncPackageMeta>(),
                    Moq.It.IsAny<string>(),
                    Moq.It.IsAny<string>()), Times.Never);

        private static InterviewSynchronizationDenormalizer denormalizer;
        private static Mock<IOrderableSyncPackageWriter<InterviewSyncPackageMeta, InterviewSyncPackageContent>> interviewPackageStorageWriterMock = new Mock<IOrderableSyncPackageWriter<InterviewSyncPackageMeta, InterviewSyncPackageContent>>();

        private static readonly Guid interviewId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
    }
}