using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answering_yesno_question_on_hard_deleted_interview : InterviewTestsContext
    {
        Establish context = () =>
        {
            command = Create.Command.AnswerYesNoQuestion();

            interview = Create.Interview();

            interview.Apply(Create.Event.InterviewHardDeleted());
        };

        Because of = () =>
            exception = Catch.Only<InterviewException>(() =>
                interview.AnswerYesNoQuestion(command));

        It should_raise_InterviewException = () =>
            exception.ShouldNotBeNull();

        It should_raise_InterviewException_with_type_InterviewHardDeleted = () =>
            exception.ExceptionType.ShouldEqual(InterviewDomainExceptionType.InterviewHardDeleted);

        private static InterviewException exception;
        private static Interview interview;
        private static AnswerYesNoQuestion command;
    }
}