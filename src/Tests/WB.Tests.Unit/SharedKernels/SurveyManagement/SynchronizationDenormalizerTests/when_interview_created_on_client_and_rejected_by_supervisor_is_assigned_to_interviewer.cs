using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.Synchronization;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SynchronizationDenormalizerTests
{
    internal class when_interview_created_on_client_and_rejected_by_supervisor_is_assigned_to_interviewer : SynchronizationDenormalizerTestsContext
    {
        Establish context = () =>
        {
            interviewId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            syncStorage = new Mock<ISynchronizationDataStorage>();

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

            synchronizationDenormalizer = CreateDenormalizer(interviews: interviews.Object,
                synchronizationDataStorage: syncStorage.Object, interviewSummaryWriter: interviewSummaryWriterMock.Object);
        };

        Because of = () => synchronizationDenormalizer.Handle(Create.InterviewerAssignedEvent());

        It should_sent_it_to_CAPI = () => syncStorage.Verify(x => x.SaveInterview(Moq.It.IsAny<InterviewSynchronizationDto>(), Moq.It.IsAny<Guid>(), Moq.It.IsAny<DateTime>()), Times.Once);

        static SynchronizationDenormalizer synchronizationDenormalizer;
        static Mock<ISynchronizationDataStorage> syncStorage;
        static Guid interviewId;
    }
}

