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
    internal class when_interview_status_changed_on_InterviewerAssigned : InterviewSynchronizationDenormalizerTestsContext
    {
        Establish context = () =>
        {
            interviewPackageStorageWriterMock = new Mock<IOrderableSyncPackageWriter<InterviewSyncPackageMetaInformation>>();

            denormalizer = CreateDenormalizer(interviewPackageStorageWriter: interviewPackageStorageWriterMock.Object);
        };

        Because of = () =>
            denormalizer.Handle(Create.Event.InterviewStatusChanged(interviewId, InterviewStatus.InterviewerAssigned));

        It should_not_create_any_packages = () =>
            interviewPackageStorageWriterMock.Verify(x => x.Store(Moq.It.IsAny<InterviewSyncPackageMetaInformation>(), Moq.It.IsAny<string>()), Times.Never);

        private static InterviewSynchronizationDenormalizer denormalizer;
        private static Mock<IOrderableSyncPackageWriter<InterviewSyncPackageMetaInformation>> interviewPackageStorageWriterMock;
        private static readonly Guid interviewId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
    }
}