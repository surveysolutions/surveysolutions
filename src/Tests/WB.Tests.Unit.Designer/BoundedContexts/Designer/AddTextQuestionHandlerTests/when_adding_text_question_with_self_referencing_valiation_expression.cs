using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.AddTextQuestionHandlerTests
{
    internal class when_adding_text_question_with_self_referencing_valiation_expression : QuestionnaireTestsContext
    {
        private Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
            questionnaire.AddTextQuestion(
                questionId: questionId,
                parentGroupId: chapterId,
                title: "title",
                variableName: variableName,
                variableLabel: null,
                isPreFilled: false,
                scope: QuestionScope.Interviewer,
                enablementCondition: "",
                validationExpression: validationExpression,
                validationMessage: "aaaa",
                instructions: "intructions",
                mask: null,
                responsibleId: responsibleId);

        It should_raise_NewQuestionAdded_event = () =>
            eventContext.ShouldContainEvent<NewQuestionAdded>();

        It should_raise_NewQuestionAdded_event_with_QuestionId_specified = () =>
            eventContext.GetSingleEvent<NewQuestionAdded>()
                .PublicKey.ShouldEqual(questionId);

        It should_raise_NewQuestionAdded_event_with_validationExpression_specified = () =>
            eventContext.GetSingleEvent<QuestionChanged>()
                .ValidationConditions.First().Expression.ShouldEqual(validationExpression);

        private static Questionnaire questionnaire;
        private static EventContext eventContext;

        private static string variableName = "var";
        private static string validationExpression = string.Format("{0} == \"Hello\"", variableName);

        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}