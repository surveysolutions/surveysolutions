using System;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.Synchronization.SyncStorage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SynchronizationDenormalizerTests
{
    internal class when_interview_is_created_on_client_and_sent_to_supervisor : InterviewSynchronizationDenormalizerTestsContext
    {
        Establish context = () =>
        {
            interviewId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var interviewSummaryWriterMock = new Mock<IReadSideRepositoryWriter<InterviewSummary>>();
            var interviewSummary = new InterviewSummary
            {
                WasCreatedOnClient = true
            };
            interviewSummary.Status = InterviewStatus.Completed;

            interviewSummaryWriterMock.SetReturnsDefault(interviewSummary);

            denormalizer = CreateDenormalizer(
                interviewPackageStorageWriter: interviewPackageStorageWriterMock.Object,
                interviewSummarys: interviewSummaryWriterMock.Object);
        };

        Because of = () => denormalizer.Handle(Create.InterviewerAssignedEvent());

        It should_not_store_any_sync_package = () =>
            interviewPackageStorageWriterMock.Verify(
                x => x.Store(
                    Moq.It.IsAny<InterviewSyncPackageContent>(),
                    Moq.It.IsAny<InterviewSyncPackageMeta>(),
                    Moq.It.IsAny<string>(),
                    Moq.It.IsAny<string>()), Times.Never);

        static InterviewSynchronizationDenormalizer denormalizer;
        static Guid interviewId;
        private static Mock<IOrderableSyncPackageWriter<InterviewSyncPackageMeta, InterviewSyncPackageContent>> interviewPackageStorageWriterMock = new Mock<IOrderableSyncPackageWriter<InterviewSyncPackageMeta, InterviewSyncPackageContent>>();

    }
}

