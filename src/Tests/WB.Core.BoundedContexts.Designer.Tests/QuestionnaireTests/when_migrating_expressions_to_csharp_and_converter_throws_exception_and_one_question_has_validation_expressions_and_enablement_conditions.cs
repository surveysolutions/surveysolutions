using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Moq;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    internal class when_migrating_expressions_to_csharp_and_converter_throws_exception_and_one_question_has_validation_expressions_and_enablement_conditions : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            Setup.SimpleQuestionnaireDocumentUpgraderToMockedServiceLocator();

            var nCalcToCSharpConverter = Mock.Of<INCalcToCSharpConverter>();

            Mock.Get(nCalcToCSharpConverter)
                .Setup(_ => _.Convert(Moq.It.IsAny<string>(), Moq.It.IsAny<Dictionary<string, string>>()))
                .Throws<Exception>();

            Setup.InstanceToMockedServiceLocator<INCalcToCSharpConverter>(nCalcToCSharpConverter);

            questionnaire = Create.Questionnaire(Create.QuestionnaireDocument(children: new[]
            {
                Create.Chapter(children: new[]
                {
                    Create.Question(enablementCondition: null, validationExpression: null),
                    Create.Question(questionId: questionId, enablementCondition: "NCalc EC", validationExpression: "NCalc VE"),
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

        It should_set_question_enablement_condition_to_initial_ncalc = () =>
            eventContext.GetSingleEvent<TemplateImported>().Source.Find<IQuestion>(questionId)
                .ConditionExpression.ShouldEqual("NCalc EC");

        It should_set_question_validation_expression_to_initial_ncalc = () =>
            eventContext.GetSingleEvent<TemplateImported>().Source.Find<IQuestion>(questionId)
                .ValidationExpression.ShouldEqual("NCalc VE");

        It should_raise_ExpressionsMigratedToCSharp_event = () =>
            eventContext.ShouldContainEvent<ExpressionsMigratedToCSharp>();

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
    }
}