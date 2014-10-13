using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    internal class when_migrating_expressions_to_csharp_and_all_questions_have_empty_expressions : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            QuestionnaireDocument questionnaireDocument = Create.QuestionnaireDocument(children: new[]
            {
                Create.Chapter(children: new[]
                {
                    Create.Question(enablementCondition: "   ", validationExpression: "   "),
                    Create.Question(enablementCondition: "", validationExpression: ""),
                    Create.Question(enablementCondition: null, validationExpression: null),
                }),
            });

            questionnaire = Create.QuestionnaireUsingQuestionnaireDocument(questionnaireDocument);

            eventContext = Create.EventContext();
        };

        Because of = () =>
            questionnaire.MigrateExpressionsToCSharp();

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_ExpressionsMigratedToCSharp_event = () =>
            eventContext.ShouldContainEvent<ExpressionsMigratedToCSharp>();

        It should_raise_only_one_event = () =>
            eventContext.Events.Count().ShouldEqual(1);

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
    }
}