using System;
using System.Linq;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_cancelling_interview_by_hq_synchronization : InterviewTestsContext
    {
        Establish context = () =>
        {
            SetupInstanceToMockedServiceLocator<IQuestionnaireRepository>(
                CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId));

            interview = CreateInterview(questionnaireId: questionnaireId);

            eventContext = new EventContext();
        };

        Because of = () =>
            interview.CancelByHQSynchronization(userId);

        It should_raise_two_events = () =>
            eventContext.Events.Count().ShouldEqual(2);

        It should_raise_InterviewDeleted_event = () =>
            eventContext.ShouldContainEvent<InterviewDeleted>();

        It should_raise_InterviewStatusChanged_event_with_Deleted_status = () =>
            eventContext.ShouldContainEvent<InterviewStatusChanged>(@event => @event.Status == InterviewStatus.Deleted);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        private static Guid userId = Guid.Parse("AAAA0000AAAA00000000AAAA0000AAAA");
        private static Guid supervisorId = Guid.Parse("BBAA0000AAAA00000000AAAA0000AAAA");

        private static Guid questionnaireId = Guid.Parse("33333333333333333333333333333333");

        private static EventContext eventContext;
        private static Interview interview;
    }
}