using System;
using System.Collections.Generic;
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
    internal class when_interview_created_on_client_and_rejected_by_supervisor_is_assigned_to_interviewer : InterviewSynchronizationDenormalizerTestsContext
    {
        Establish context = () =>
        {
            interviewId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            InterviewData data = Create.InterviewData(createdOnClient: true, 
                status: InterviewStatus.RejectedBySupervisor,
                interviewId: interviewId);

            var interviews = new Mock<IReadSideKeyValueStorage<InterviewData>>();
            interviews.SetReturnsDefault(data);


            var interviewSummaryWriterMock = new Mock<IReadSideRepositoryWriter<InterviewSummary>>();
            interviewSummaryWriterMock.SetReturnsDefault(new InterviewSummary()
            {
                WasCreatedOnClient = true,
                CommentedStatusesHistory =
                    new List<InterviewCommentedStatus>
                                    {
                                        new InterviewCommentedStatus() { Status = InterviewStatus.RejectedBySupervisor }
                                    }
            });

            interviewPackageStorageWriter = new Mock<IOrderableSyncPackageWriter<InterviewSyncPackage>>();

            synchronizationDenormalizer = CreateDenormalizer(
                interviews: interviews.Object,
                interviewPackageStorageWriter: interviewPackageStorageWriter.Object, 
                interviewSummarys: interviewSummaryWriterMock.Object);
        };

        Because of = () => synchronizationDenormalizer.Handle(Create.InterviewerAssignedEvent());

        It should_sent_it_to_CAPI = () => 
            interviewPackageStorageWriter.Verify(x => 
                x.Store(Moq.It.IsAny<InterviewSyncPackage>(), Moq.It.IsAny<string>()), Times.Once);

        static InterviewSynchronizationDenormalizer synchronizationDenormalizer;
        static Guid interviewId;
        private static Mock<IOrderableSyncPackageWriter<InterviewSyncPackage>> interviewPackageStorageWriter;
    }
}

