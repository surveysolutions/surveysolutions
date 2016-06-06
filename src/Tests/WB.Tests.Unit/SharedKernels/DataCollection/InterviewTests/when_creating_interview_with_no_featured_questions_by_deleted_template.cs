using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_creating_interview_with_no_featured_questions_by_deleted_template : InterviewTestsContext
    {
        Establish context = () =>
        {
            eventContext = new EventContext();

            interview = Create.AggregateRoot.Interview();
        };

        Because of = () => exception = Catch.Exception(() =>
            interview.CreateInterviewCreatedOnClient(questionnaireId, questionnaireVersion, interviewStatus, featuredQuestionsMeta, isValid, userId));

        It should_event_context_contains__events = () =>
            eventContext.Events.Count().ShouldEqual(0);

        It should_not_raise_InterviewCreated_event = () =>
            eventContext.ShouldNotContainEvent<InterviewOnClientCreated>();

        It should_raise_InterviewException = () =>
            exception.ShouldBeOfExactType(typeof(InterviewException));

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        private static EventContext eventContext;
        private static Exception exception;
        private static Guid questionnaireId = Guid.Parse("10000000000000000000000000000000");
        private static long questionnaireVersion = 18;
        private static Guid userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static bool isValid = true;
        private static AnsweredQuestionSynchronizationDto[] featuredQuestionsMeta = null;
        private static InterviewStatus interviewStatus = InterviewStatus.Completed;
        private static Interview interview;
    }
}
