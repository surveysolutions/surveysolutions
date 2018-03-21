using System.Collections.Generic;
using FluentAssertions;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answering_yesno_question_on_hard_deleted_interview
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            command = Create.Command.AnswerYesNoQuestion();

            interview = Create.AggregateRoot.Interview();

            interview.Apply(Create.Event.InterviewHardDeleted());
            BecauseOf();
        }

        public void BecauseOf() =>
            exception =  NUnit.Framework.Assert.Throws<InterviewException>(() =>
                interview.AnswerYesNoQuestion(command));

        [NUnit.Framework.Test] public void should_throw_InterviewException () =>
            exception.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_throw_InterviewException_with_type_InterviewHardDeleted () =>
            exception.ExceptionType.Should().Be(InterviewDomainExceptionType.InterviewHardDeleted);

        private static AnswerYesNoQuestion command;
        private static Interview interview;
        private static InterviewException exception;
    }
}
