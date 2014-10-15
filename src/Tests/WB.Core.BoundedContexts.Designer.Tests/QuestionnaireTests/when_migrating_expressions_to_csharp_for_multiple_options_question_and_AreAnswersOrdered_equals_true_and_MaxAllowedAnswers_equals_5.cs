using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    internal class when_migrating_expressions_to_csharp_for_multiple_options_question_and_AreAnswersOrdered_equals_true_and_MaxAllowedAnswers_equals_5 : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            Setup.SimpleQuestionnaireDocumentUpgraderToMockedServiceLocator();

            questionnaire = Create.Questionnaire(Create.QuestionnaireDocument(children: new[]
            {
                Create.Chapter(children: new[]
                {
                    Create.MultipleOptionsQuestion(enablementCondition: "EC", areAnswersOrdered: true, maxAllowedAnswers: 5),
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

        It should_raise_1_QuestionChanged_event = () =>
            eventContext.ShouldContainEvents<QuestionChanged>(count: 1);

        It should_raise_QuestionChanged_event_with_AreAnswersOrdered_set_to_true = () =>
            eventContext.GetSingleEvent<QuestionChanged>()
                .AreAnswersOrdered.ShouldEqual(true);

        It should_raise_QuestionChanged_event_with_MaxAllowedAnswers_set_to_5 = () =>
            eventContext.GetSingleEvent<QuestionChanged>()
                .MaxAllowedAnswers.ShouldEqual(5);

        It should_raise_ExpressionsMigratedToCSharp_event = () =>
            eventContext.ShouldContainEvent<ExpressionsMigratedToCSharp>();

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
    }
}