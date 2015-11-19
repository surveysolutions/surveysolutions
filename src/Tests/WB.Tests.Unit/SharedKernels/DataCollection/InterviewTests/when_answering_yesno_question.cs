using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answering_yesno_question
    {
        Establish context = () =>
        {
            var questionnaireDocument = Create.QuestionnaireDocument(children: new[]
            {
                Create.YesNoQuestion(questionId: questionId),
            });

            interview = Setup.InterviewForQuestionnaireDocument(questionnaireDocument);

            command = Create.Command.AnswerYesNoQuestion(questionId: questionId);

            eventContext = Create.EventContext();
        };

        Because of = () =>
            interview.AnswerYesNoQuestion(command);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_YesNoQuestionAnswered_event = () =>
            eventContext.ShouldContainEvent<YesNoQuestionAnswered>();

        private static AnswerYesNoQuestion command;
        private static Interview interview;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static EventContext eventContext;
    }
}