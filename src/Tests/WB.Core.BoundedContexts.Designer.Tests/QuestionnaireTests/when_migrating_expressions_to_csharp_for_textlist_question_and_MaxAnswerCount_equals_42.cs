using Machine.Specifications;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    internal class when_migrating_expressions_to_csharp_for_textlist_question_and_MaxAnswerCount_equals_42 : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            Setup.SimpleQuestionnaireDocumentUpgraderToMockedServiceLocator();

            questionnaire = Create.Questionnaire(Create.QuestionnaireDocument(children: new[]
            {
                Create.Chapter(children: new[]
                {
                    Create.TextListQuestion(enablementCondition: "EC", maxAnswerCount: 42),
                }),
            }));

            eventContext = Create.EventContext();
        };

        Because of = () =>
            questionnaire.MigrateExpressionsToCSharp();

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_1_TextListQuestionChanged_event = () =>
            eventContext.ShouldContainEvents<TextListQuestionChanged>(count: 1);

        It should_raise_TextListQuestionChanged_event_with_MaxAnswerCount_set_to_42 = () =>
            eventContext.GetSingleEvent<TextListQuestionChanged>()
                .MaxAnswerCount.ShouldEqual(42);

        It should_raise_ExpressionsMigratedToCSharp_event = () =>
            eventContext.ShouldContainEvent<ExpressionsMigratedToCSharp>();

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
    }
}