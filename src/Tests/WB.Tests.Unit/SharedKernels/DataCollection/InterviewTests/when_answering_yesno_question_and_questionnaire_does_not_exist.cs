using System;
using FluentAssertions;
using Moq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answering_yesno_question_and_questionnaire_does_not_exist
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            command = Create.Command.AnswerYesNoQuestion();

            var repositoryWithoutQuestionnaire = Mock.Of<IQuestionnaireStorage>();

            interview = Create.AggregateRoot.Interview(questionnaireRepository: repositoryWithoutQuestionnaire);

            interview.Apply(Create.Event.InterviewCreated(questionnaireId: questionnaireId, questionnaireVersion: questionnaireVersion));
            BecauseOf();
        }

        public void BecauseOf() =>
            exception =  NUnit.Framework.Assert.Throws<InterviewException>(() =>
                interview.AnswerYesNoQuestion(command));

        [NUnit.Framework.Test] public void should_throw_InterviewException () =>
            exception.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containing__questionnaire____not_____found__ () =>
            exception.Message.ToLower().ToSeparateWords().Should().Contain("questionnaire", "not", "found");

        private static AnswerYesNoQuestion command;
        private static Interview interview;
        private static InterviewException exception;
        private static Guid questionnaireId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static long questionnaireVersion = 3;
    }
}
