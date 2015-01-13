using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.Synchronization;
using WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SynchronizationDenormalizerTests
{
    internal class when_interview_is_created_on_client_and_sent_to_supervisor : SynchronizationDenormalizerTestsContext
    {
        Establish context = () =>
        {
            interviewId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var interviewSummaryWriterMock = new Mock<IReadSideRepositoryWriter<InterviewSummary>>();
            interviewSummaryWriterMock.SetReturnsDefault(new InterviewSummary()
            {
                WasCreatedOnClient = true,
                CommentedStatusesHistory =
                    new List<InterviewCommentedStatus>
                                    {
                                        new InterviewCommentedStatus() { Status = InterviewStatus.Completed }
                                    }
            });

            syncStorage = new Mock<ISynchronizationDataStorage>();

            denormalizer = CreateDenormalizer(synchronizationDataStorage: syncStorage.Object, interviewSummaryWriter: interviewSummaryWriterMock.Object);
        };

        Because of = () => denormalizer.Handle(Create.InterviewerAssignedEvent());

        It should_not_send_interview_back_to_tablet = () => 
            syncStorage.Verify(x => 
                    x.SaveInterview(Moq.It.IsAny<InterviewSynchronizationDto>(), Moq.It.IsAny<Guid>(), Moq.It.IsAny<DateTime>()),
            Times.Never);

        static SynchronizationDenormalizer denormalizer;
        static Mock<ISynchronizationDataStorage> syncStorage;
        static Guid interviewId;
    }
}

