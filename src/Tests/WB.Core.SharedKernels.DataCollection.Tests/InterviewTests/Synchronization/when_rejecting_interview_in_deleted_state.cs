using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests.Synchronization
{
    internal class when_rejecting_interview_from_HQ_in_deleted_state : InterviewTestsContext
    {
        Establish context = () =>
        {
            interview = CreateInterview();
            interview.Apply(new InterviewStatusChanged(InterviewStatus.Deleted, null));
            var questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.GetHistoricalQuestionnaire(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>()))
                .Returns(Mock.Of<IQuestionnaire>());

            SetupInstanceToMockedServiceLocator<IQuestionnaireRepository>(questionnaireRepositoryMock.Object);

            eventContext = new EventContext();
        };

        Because of = () => interview.RejectInterviewFromHeadquarters(userId, Guid.NewGuid(), Guid.NewGuid(), new InterviewSynchronizationDto(), DateTime.Now);

        It should_restore_interview = () => eventContext.ShouldContainEvent<InterviewRestored>(@event => @event.UserId == userId);

        static Interview interview;
        static EventContext eventContext;
        static Guid userId = Guid.NewGuid();
    }
}