using System;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable;
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
            command = Create.Command.AnswerYesNoQuestion(questionId: Guid.Parse("11111111111111111111111111111111"));

            var questionnaire = Mock.Of<IQuestionnaire>
            (_
                => _.HasQuestion(command.QuestionId) == false
            );

            interview = Setup.InterviewForQuestionnaire(questionnaire);
        };

        Because of = () =>
            exception = Catch.Only<InterviewException>(() =>
                interview.AnswerYesNoQuestion(command));

        It should_throw_InterviewException = () =>
            exception.ShouldNotBeNull();

        It should_throw_exception_with_message_containing__question____missing__ = () =>
            exception.Message.ToLower().ToSeparateWords().ShouldContain("question", "missing");

        It should_throw_exception_with_message_containing_question_id_from_command = () =>
            exception.Message.ShouldContain(command.QuestionId.FormatGuid());

        private static AnswerYesNoQuestion command;
        private static Interview interview;
        private static InterviewException exception;
    }
}