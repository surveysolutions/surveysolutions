using System;
using FluentAssertions;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answering_yesno_question_which_does_not_exist
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            command = Create.Command.AnswerYesNoQuestion(questionId: Guid.Parse("11111111111111111111111111111111"));

            var questionnaire = Mock.Of<IQuestionnaire>
            (_
                => _.HasQuestion(command.QuestionId) == false
            );

            interview = Setup.InterviewForQuestionnaire(questionnaire);
            BecauseOf();
        }

        public void BecauseOf() =>
            exception =  NUnit.Framework.Assert.Throws<InterviewException>(() =>
                interview.AnswerYesNoQuestion(command));

        [NUnit.Framework.Test] public void should_throw_InterviewException () =>
            exception.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containing__question____missing__ () =>
            exception.Message.ToLower().ToSeparateWords().Should().Contain("question", "missing");

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containing_question_id_from_command () =>
            exception.Message.Should().Contain(command.QuestionId.FormatGuid());

        private static AnswerYesNoQuestion command;
        private static Interview interview;
        private static InterviewException exception;
    }
}
