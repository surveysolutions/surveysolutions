using System;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.Synchronization.SyncStorage;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SynchronizationDenormalizerTests
{
    internal class when_interview_created_on_client_and_assigned_to_interviewer : InterviewSynchronizationDenormalizerTestsContext
    {
        Establish context = () =>
        {
            interviewId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            interviewPackageStorageWriter = new Mock<IOrderableSyncPackageWriter<InterviewSyncPackage>>();

            denormalizer = CreateDenormalizer(interviewPackageStorageWriter: interviewPackageStorageWriter.Object);
        };


        Because of = () => denormalizer.Handle(Create.InterviewStatusChangedEvent(InterviewStatus.Completed));

        It should_not_send_deletion_package_to_tablets = () =>
            interviewPackageStorageWriter.Verify(x => x.Store(Moq.It.IsAny<InterviewSyncPackage>(), Moq.It.IsAny<string>()), Times.Never);

        static InterviewSynchronizationDenormalizer denormalizer;
        static Mock<IOrderableSyncPackageWriter<InterviewSyncPackage>> interviewPackageStorageWriter;
        static Guid interviewId;
    }
}

