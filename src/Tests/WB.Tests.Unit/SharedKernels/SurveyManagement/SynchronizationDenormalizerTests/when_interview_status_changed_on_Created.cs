using System;

using Machine.Specifications;

using Moq;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.Synchronization.SyncStorage;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SynchronizationDenormalizerTests
{
    internal class when_interview_status_changed_on_Created : InterviewSynchronizationDenormalizerTestsContext
    {
        Establish context = () =>
        {
            denormalizer = CreateDenormalizer(interviewPackageStorageWriter: interviewPackageStorageWriterMock.Object);
        };

        Because of = () =>
            denormalizer.Handle(Create.PublishedEvent.InterviewStatusChanged(interviewId, InterviewStatus.Created));

        It should_not_store_any_sync_package = () =>
            interviewPackageStorageWriterMock.Verify(
                x => x.Store(
                    Moq.It.IsAny<InterviewSyncPackageMeta>(),
                    Moq.It.IsAny<string>()), Times.Never);

        private static InterviewSynchronizationDenormalizer denormalizer;
        private static Mock<IReadSideRepositoryWriter<InterviewSyncPackageMeta>> interviewPackageStorageWriterMock = new Mock<IReadSideRepositoryWriter<InterviewSyncPackageMeta>>();

        private static readonly Guid interviewId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
    }
}