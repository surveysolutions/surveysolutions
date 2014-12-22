extern alias designer;

using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Moq;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using It = Machine.Specifications.It;

using TemplateImported = designer::Main.Core.Events.Questionnaire.TemplateImported;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_migrating_expressions_to_csharp_and_2_questions_B_and_D_both_have_validation_expressions_and_enablement_conditions :
        QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            Setup.SimpleQuestionnaireDocumentUpgraderToMockedServiceLocator();

            Setup.InstanceToMockedServiceLocator(Mock.Of<INCalcToCSharpConverter>(_
                => _.Convert("NCalc EC B", Moq.It.IsAny<Dictionary<string, string>>()) == "C# EC B"
                && _.Convert("NCalc EC D", Moq.It.IsAny<Dictionary<string, string>>()) == "C# EC D"
                && _.Convert("NCalc VE B", Moq.It.IsAny<Dictionary<string, string>>()) == "C# VE B"
                && _.Convert("NCalc VE D", Moq.It.IsAny<Dictionary<string, string>>()) == "C# VE D"));

            questionnaire = Create.Questionnaire(Create.QuestionnaireDocument(children: new[]
            {
                Create.Chapter(children: new[]
                {
                    Create.Question(enablementCondition: null, validationExpression: null),
                    Create.Question(questionId: questionBId, enablementCondition: "NCalc EC B", validationExpression: "NCalc VE B"),
                    Create.Question(enablementCondition: null, validationExpression: null),
                    Create.Question(questionId: questionDId, enablementCondition: "NCalc EC D", validationExpression: "NCalc VE D"),
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

        It should_raise_TemplateImported_event = () =>
            eventContext.ShouldContainEvent<TemplateImported>();

        It should_set_question_B_enablement_condition_to_converted_to_csharp = () =>
            eventContext.GetSingleEvent<TemplateImported>().Source.Find<IQuestion>(questionBId)
                .ConditionExpression.ShouldEqual("C# EC B");

        It should_set_question_B_validation_expression_to_converted_to_csharp = () =>
            eventContext.GetSingleEvent<TemplateImported>().Source.Find<IQuestion>(questionBId)
                .ValidationExpression.ShouldEqual("C# VE B");

        It should_set_question_D_enablement_condition_to_converted_to_csharp = () =>
            eventContext.GetSingleEvent<TemplateImported>().Source.Find<IQuestion>(questionDId)
                .ConditionExpression.ShouldEqual("C# EC D");

        It should_set_question_D_validation_expression_to_converted_to_csharp = () =>
            eventContext.GetSingleEvent<TemplateImported>().Source.Find<IQuestion>(questionDId)
                .ValidationExpression.ShouldEqual("C# VE D");

        It should_raise_ExpressionsMigratedToCSharp_event = () =>
            eventContext.ShouldContainEvent<ExpressionsMigratedToCSharp>();

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid questionBId = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
        private static Guid questionDId = Guid.Parse("dddddddddddddddddddddddddddddddd");
    }
}