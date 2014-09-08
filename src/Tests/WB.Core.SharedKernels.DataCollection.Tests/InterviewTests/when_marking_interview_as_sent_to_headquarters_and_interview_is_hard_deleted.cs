using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_marking_interview_as_sent_to_headquarters_and_interview_is_hard_deleted : InterviewTestsContext
    {
        Establish context = () =>
        {
            SetupInstanceToMockedServiceLocator(new Mock<IQuestionnaireRepository> { DefaultValue = DefaultValue.Mock }.Object);

            interview = CreateInterview();

            interview.Apply(new InterviewHardDeleted(userId));

            eventContext = new EventContext();
        };

        Because of = () =>
            interview.MarkInterviewAsSentToHeadquarters(userId);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_not_raise_InterviewStatusChanged_event = () =>
            eventContext.ShouldNotContainEvent<InterviewStatusChanged>();

        It should_not_raise_InterviewStatusChanged_event_with_Deleted_status = () =>
            eventContext.ShouldNotContainEvent<InterviewStatusChanged>(@event
                => @event.Status == InterviewStatus.Deleted);

        It should_not_raise_InterviewDeleted_event = () =>
            eventContext.ShouldNotContainEvent<InterviewDeleted>();

        It should_raise_InterviewSentToHeadquarters_event = () =>
            eventContext.ShouldContainEvent<InterviewSentToHeadquarters>();

        private static Interview interview;
        private static Guid userId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static EventContext eventContext;
    }
}
