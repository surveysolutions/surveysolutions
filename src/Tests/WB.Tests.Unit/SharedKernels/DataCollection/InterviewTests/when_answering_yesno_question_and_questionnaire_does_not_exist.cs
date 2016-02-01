using System;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answering_yesno_question_and_questionnaire_does_not_exist
    {
        Establish context = () =>
        {
            command = Create.Command.AnswerYesNoQuestion();

            var repositoryWithoutQuestionnaire = Mock.Of<IPlainQuestionnaireRepository>(repository
                => repository.GetHistoricalQuestionnaire(questionnaireId, questionnaireVersion) == null as IQuestionnaire);

            interview = Create.Interview(questionnaireRepository: repositoryWithoutQuestionnaire);

            interview.Apply(Create.Event.InterviewCreated(questionnaireId: questionnaireId, questionnaireVersion: questionnaireVersion));
        };

        Because of = () =>
            exception = Catch.Only<InterviewException>(() =>
                interview.AnswerYesNoQuestion(command));

        It should_throw_InterviewException = () =>
            exception.ShouldNotBeNull();

        It should_throw_exception_with_message_containing__questionnaire____not_____found__ = () =>
            exception.Message.ToLower().ToSeparateWords().ShouldContain("questionnaire", "not", "found");

        private static AnswerYesNoQuestion command;
        private static Interview interview;
        private static InterviewException exception;
        private static Guid questionnaireId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static long questionnaireVersion = 3;
    }
}