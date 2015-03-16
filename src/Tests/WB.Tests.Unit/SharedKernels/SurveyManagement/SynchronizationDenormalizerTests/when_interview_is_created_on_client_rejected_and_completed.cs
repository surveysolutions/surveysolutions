using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.Synchronization.SyncStorage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SynchronizationDenormalizerTests
{
    internal class when_interview_is_created_on_client_rejected_and_completed : InterviewSynchronizationDenormalizerTestsContext
    {
        Establish context = () =>
        {
            interviewId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            InterviewData data = Create.InterviewData(createdOnClient: true, 
                status: InterviewStatus.Completed,
                interviewId: interviewId);

            var interviews = new Mock<IReadSideKeyValueStorage<InterviewData>>();
            interviews.SetReturnsDefault(data);

            var synchronizationDto = CreateSynchronizationDto(interviewId);

            var synchronizationDtoFactory = Mock.Of<IInterviewSynchronizationDtoFactory>(
                    x => x.BuildFrom(data, Moq.It.IsAny<Guid>(), InterviewStatus.RejectedBySupervisor, Moq.It.IsAny<string>()) == synchronizationDto);
            
            var interviewSummaryWriterMock = new Mock<IReadSideRepositoryWriter<InterviewSummary>>();
            var interviewSummary = new InterviewSummary
            {
                WasCreatedOnClient = true
            };
            interviewSummary.CommentedStatusesHistory.Add(
                new InterviewCommentedStatus { Status = InterviewStatus.RejectedBySupervisor }
                );

            interviewSummaryWriterMock.SetReturnsDefault(interviewSummary);

            synchronizationDenormalizer = CreateDenormalizer(
                interviews: interviews.Object,
                interviewPackageStorageWriter: interviewPackageStorageWriter.Object,
                interviewSummarys: interviewSummaryWriterMock.Object,
                synchronizationDtoFactory: synchronizationDtoFactory);
        };

        Because of = () =>
        {
            synchronizationDenormalizer.Handle(Create.InterviewStatusChangedEvent(InterviewStatus.RejectedBySupervisor, interviewId: interviewId));
            synchronizationDenormalizer.Handle(Create.InterviewStatusChangedEvent(InterviewStatus.Completed, interviewId: interviewId));
        };

        It should_create_deletion_synchronization_package = () =>
            interviewPackageStorageWriter.Verify(x => 
                x.Store(Moq.It.IsAny<InterviewSyncPackageContent>(),
                Moq.It.Is<InterviewSyncPackageMeta>(s => s.InterviewId == interviewId),
                Moq.It.IsAny<string>(),
                CounterId), Times.Exactly(2));

        static InterviewSynchronizationDenormalizer synchronizationDenormalizer;
        static Guid interviewId;
        private static Mock<IOrderableSyncPackageWriter<InterviewSyncPackageMeta, InterviewSyncPackageContent>> interviewPackageStorageWriter = new Mock<IOrderableSyncPackageWriter<InterviewSyncPackageMeta, InterviewSyncPackageContent>>();
    }
}