using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
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


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_synchronizing_interview_events_but_interview_was_reassigned_to_supervisor_in_other_team : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            eventContext = new EventContext();
            var questionnaire = Mock.Of<IQuestionnaire>(_ => _.Version == questionnaireVersion);

            var questionnaireRepository = Stub<IQuestionnaireStorage>.Returning(questionnaire);

            interview = Create.AggregateRoot.Interview(questionnaireRepository: questionnaireRepository);
            interview.Apply(new InterviewStatusChanged(InterviewStatus.InterviewerAssigned, "", DateTimeOffset.Now));
            interview.Apply(new InterviewerAssigned(interviewerId, interviewerId, DateTime.Now));
            interview.Apply(new SupervisorAssigned(supervisorId, supervisorId, DateTimeOffset.Now));
            interview.Apply(new InterviewerAssigned(supervisorId, null, DateTime.Now));
            command = Create.Command.SynchronizeInterviewEventsCommand(
               userId: Guid.NewGuid(),
               questionnaireId: questionnaireId,
               questionnaireVersion: questionnaireVersion,
               synchronizedEvents: eventsToPublish,
               createdOnClient: false);
            BecauseOf();
        }

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        public void BecauseOf() =>
            exception =  NUnit.Framework.Assert.Throws<InterviewException>(() => interview.SynchronizeInterviewEvents(command));

        [NUnit.Framework.Test] public void should_raise_InterviewException () =>
           exception.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_raise_InterviewException_with_type_OtherUserIsResponsible () =>
            exception.ExceptionType.Should().Be(InterviewDomainExceptionType.OtherUserIsResponsible);

        static EventContext eventContext;
        static Guid questionnaireId = Guid.Parse("10000000000000000000000000000000");
        static Guid interviewerId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        static Guid supervisorId  = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        static long questionnaireVersion = 18;

        static Interview interview;

        static InterviewException exception;

        static IEvent[] eventsToPublish = new IEvent[]
        {
            new AnswersDeclaredInvalid(
                new Identity[0].ToDictionary<Identity, Identity, IReadOnlyList<FailedValidationCondition>>(question => question, question => new List<FailedValidationCondition>()), 
                DateTimeOffset.Now),
            new GroupsEnabled(new Identity[0], DateTimeOffset.Now)
        };
        static SynchronizeInterviewEventsCommand command;
    }
}
