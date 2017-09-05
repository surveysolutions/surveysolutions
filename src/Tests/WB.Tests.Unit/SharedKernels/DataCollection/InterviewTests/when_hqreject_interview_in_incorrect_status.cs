using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_hqreject_interview_in_incorrect_status : InterviewTestsContext
    {
        private Establish context = () =>
        {
            userId = Guid.Parse("AAAA0000AAAA00000000AAAA0000AAAA");
            questionnaireId = Guid.Parse("33333333333333333333333333333333");

            interview = Create.AggregateRoot.StatefulInterview(questionnaire: Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId));

            eventContext = new EventContext();
        };

        Because of = () =>
            exception = Catch.Only<InterviewException>(() => interview.HqReject(userId, String.Empty));

        It should_raise_InterviewException = () =>
            exception.ShouldNotBeNull();

        It should_raise_InterviewException_with_type_StatusIsNotOneOfExpected = () =>
            exception.ExceptionType.ShouldEqual(InterviewDomainExceptionType.StatusIsNotOneOfExpected);
        
        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        private static Guid userId;

        private static InterviewException exception;
        private static Guid questionnaireId;
        private static EventContext eventContext;
        private static Interview interview;
    }
}
