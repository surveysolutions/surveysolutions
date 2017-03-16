using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Spec;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_synchronizing_interview_events_but_interview_was_reassigned_to_supervisor_in_other_team : InterviewTestsContext
    {
        Establish context = () =>
        {
            eventContext = new EventContext();
            var questionnaire = Mock.Of<IQuestionnaire>(_ => _.Version == questionnaireVersion);

            var questionnaireRepository = Stub<IQuestionnaireStorage>.Returning(questionnaire);

            interview = Create.AggregateRoot.Interview(questionnaireRepository: questionnaireRepository);
            interview.Apply(new InterviewStatusChanged(InterviewStatus.InterviewerAssigned, ""));
            interview.Apply(new InterviewerAssigned(interviewerId, interviewerId, DateTime.Now));
            interview.Apply(new SupervisorAssigned(supervisorId, supervisorId));
            interview.Apply(new InterviewerAssigned(supervisorId, null, DateTime.Now));
            command = Create.Command.SynchronizeInterviewEventsCommand(
               userId: Guid.NewGuid(),
               questionnaireId: questionnaireId,
               questionnaireVersion: questionnaireVersion,
               synchronizedEvents: eventsToPublish,
               createdOnClient: false);
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
            exception = Catch.Only<InterviewException>(() => interview.SynchronizeInterviewEvents(command));

        It should_raise_InterviewException = () =>
           exception.ShouldNotBeNull();

        It should_raise_InterviewException_with_type_OtherUserIsResponsible = () =>
            exception.ExceptionType.ShouldEqual(InterviewDomainExceptionType.OtherUserIsResponsible);

        static EventContext eventContext;
        static Guid questionnaireId = Guid.Parse("10000000000000000000000000000000");
        static Guid interviewerId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        static Guid supervisorId  = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        static long questionnaireVersion = 18;

        static Interview interview;

        static InterviewException exception;

        static IEvent[] eventsToPublish = new IEvent[] { new AnswersDeclaredInvalid(new Identity[0]), new GroupsEnabled(new Identity[0]) };
        static SynchronizeInterviewEventsCommand command;
    }
}
