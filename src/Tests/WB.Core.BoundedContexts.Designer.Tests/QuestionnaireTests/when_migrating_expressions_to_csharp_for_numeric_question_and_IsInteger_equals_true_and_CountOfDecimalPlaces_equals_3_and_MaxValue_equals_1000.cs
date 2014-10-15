using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    internal class when_migrating_expressions_to_csharp_for_numeric_question_and_IsInteger_equals_true_and_CountOfDecimalPlaces_equals_3_and_MaxValue_equals_1000 : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            Setup.SimpleQuestionnaireDocumentUpgraderToMockedServiceLocator();

            questionnaire = Create.Questionnaire(Create.QuestionnaireDocument(children: new[]
            {
                Create.Chapter(children: new[]
                {
                    Create.NumericQuestion(enablementCondition: "EC", isInteger: true, countOfDecimalPlaces: 3, maxValue: 1000),
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

        It should_raise_1_NumericQuestionChanged_event = () =>
            eventContext.ShouldContainEvents<NumericQuestionChanged>(count: 1);

        It should_raise_NumericQuestionChanged_event_with_IsInteger_set_to_true = () =>
            eventContext.GetSingleEvent<NumericQuestionChanged>()
                .IsInteger.ShouldEqual(true);

        It should_raise_NumericQuestionChanged_event_with_CountOfDecimalPlaces_set_to_3 = () =>
            eventContext.GetSingleEvent<NumericQuestionChanged>()
                .CountOfDecimalPlaces.ShouldEqual(3);

        It should_raise_NumericQuestionChanged_event_with_MaxValue_set_to_1000 = () =>
            eventContext.GetSingleEvent<NumericQuestionChanged>()
                .MaxAllowedValue.ShouldEqual(1000);

        It should_raise_ExpressionsMigratedToCSharp_event = () =>
            eventContext.ShouldContainEvent<ExpressionsMigratedToCSharp>();

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
    }
}