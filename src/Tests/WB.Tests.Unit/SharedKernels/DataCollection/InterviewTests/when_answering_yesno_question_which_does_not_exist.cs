using System;
using FluentAssertions;
using Moq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answering_yesno_question_which_does_not_exist
    {
        [NUnit.Framework.Test]
        public void should_throw_exception_with_message_containing__question____missing__()
        {
            var command = Create.Command.AnswerYesNoQuestion(questionId: Guid.Parse("11111111111111111111111111111111"));

            var questionnaire = Mock.Of<IQuestionnaire>
            (_
                => _.HasQuestion(command.QuestionId) == false
            );

            var interview = Setup.InterviewForQuestionnaire(questionnaire);

            // Act
            var exception = NUnit.Framework.Assert.Throws<InterviewException>(() =>
               interview.AnswerYesNoQuestion(command));

            // Assert
            exception.Message.Should().Be("Question is missing");
        }
    }
}
