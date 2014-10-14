using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using Moq;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    internal class when_migrating_expressions_to_csharp_and_question_has_validation_expression_but_enablement_condition_is_empty :
        QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            Setup.SimpleQuestionnaireDocumentUpgraderToMockedServiceLocator();

            Setup.InstanceToMockedServiceLocator(Mock.Of<INCalcToCSharpConverter>(_
                => _.Convert("NCalc VE", Moq.It.IsAny<Dictionary<string, string>>()) == "C# VE"));

            questionnaire = Create.Questionnaire(Create.QuestionnaireDocument(children: new[]
            {
                Create.Chapter(children: new[]
                {
                    Create.Question(enablementCondition: null, validationExpression: null),
                    Create.Question(enablementCondition: " ", validationExpression: "NCalc VE"),
                    Create.Question(enablementCondition: null, validationExpression: null),
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

        It should_raise_QuestionChanged_event_for_question_with_not_changed_enablement_condition_and_converted_validation_expression = () =>
            eventContext.ShouldContainEvent<QuestionChanged>(@event
                => @event.ConditionExpression == " "
                && @event.ValidationExpression == "C# VE");

        It should_raise_ExpressionsMigratedToCSharp_event = () =>
            eventContext.ShouldContainEvent<ExpressionsMigratedToCSharp>();

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
    }
}