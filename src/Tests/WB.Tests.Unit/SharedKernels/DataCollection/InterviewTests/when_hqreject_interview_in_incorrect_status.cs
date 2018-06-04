using System;
using FluentAssertions;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_hqreject_interview_in_incorrect_status : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            userId = Guid.Parse("AAAA0000AAAA00000000AAAA0000AAAA");
            questionnaireId = Guid.Parse("33333333333333333333333333333333");

            interview = Create.AggregateRoot.StatefulInterview(questionnaire: Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId));

            eventContext = new EventContext();
            BecauseOf();
        }

        public void BecauseOf() =>
            exception =  NUnit.Framework.Assert.Throws<InterviewException>(() => interview.HqReject(userId, String.Empty, DateTimeOffset.Now));

        [NUnit.Framework.Test] public void should_raise_InterviewException () =>
            exception.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_raise_InterviewException_with_type_StatusIsNotOneOfExpected () =>
            exception.ExceptionType.Should().Be(InterviewDomainExceptionType.StatusIsNotOneOfExpected);
        
        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        private static Guid userId;

        private static InterviewException exception;
        private static Guid questionnaireId;
        private static EventContext eventContext;
        private static Interview interview;
    }
}
