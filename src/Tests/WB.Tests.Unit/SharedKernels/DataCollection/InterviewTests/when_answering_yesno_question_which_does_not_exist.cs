using System;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answering_yesno_question_which_does_not_exist
    {
        Establish context = () =>
        {
            var questionnaire = Mock.Of<IQuestionnaire>
            (_
                => _.HasQuestion(questionId) == false
            );

            interview = Setup.InterviewForQuestionnaire(questionnaire);

            command = Create.Command.AnswerYesNoQuestion(questionId: questionId);
        };

        Because of = () =>
            exception = Catch.Only<InterviewException>(() =>
                interview.AnswerYesNoQuestion(command));

        It should_throw_InterviewException = () =>
            exception.ShouldNotBeNull();

        It should_throw_exception_with_message_containing__question____not_____found__ = () =>
            exception.Message.ToLower().ToSeparateWords().ShouldContain("question", "not", "found");

        private static AnswerYesNoQuestion command;
        private static Interview interview;
        private static InterviewException exception;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
    }
}