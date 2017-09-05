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
    internal class when_synchronizing_interview_events_but_for_interview_is_responsible_other_user : InterviewTestsContext
    {
        Establish context = () =>
        {
            eventContext = new EventContext();
            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.Version == questionnaireVersion);

            var questionnaireRepository = Stub<IQuestionnaireStorage>.Returning(questionnaire);

            interview = Create.AggregateRoot.Interview(questionnaireRepository: questionnaireRepository);
            interview.Apply(new InterviewStatusChanged(InterviewStatus.InterviewerAssigned, ""));
            interview.Apply(new InterviewerAssigned(userId, userId, DateTime.Now));

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
        static readonly Guid questionnaireId = Guid.Parse("10000000000000000000000000000000");
        static readonly Guid userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        static long questionnaireVersion = 18;

        static Interview interview;

        static InterviewException exception;

        static readonly IEvent[] eventsToPublish = new IEvent[] { new AnswersDeclaredInvalid(new Identity[0]), new GroupsEnabled(new Identity[0]) };
        static SynchronizeInterviewEventsCommand command;
    }
}
