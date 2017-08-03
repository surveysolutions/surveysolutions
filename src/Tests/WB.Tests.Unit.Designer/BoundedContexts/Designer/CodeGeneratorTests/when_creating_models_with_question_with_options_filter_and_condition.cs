using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.CodeGeneratorTests
{
    internal class when_creating_models_with_question_with_options_filter_and_condition : CodeGeneratorTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            AssemblyContext.SetupServiceLocator();

            questionnaire = Create.QuestionnaireDocument(children: new[]
            {
                Create.Chapter(children: new IComposite[]
                {
                    Create.NumericIntegerQuestion(variable: "num"),
                    Create.SingleQuestion(variable: "singleFiltered", optionsFilter: "num > 1", enablementCondition: "num != 5", options: new List<Answer>
                    {
                        Create.Option("1", "Option 1"),
                        Create.Option("2", "Option 2")
                    })
                })
            });

            templateModelFactory = Create.QuestionnaireExecutorTemplateModelFactory(expressionProcessor: new RoslynExpressionProcessor());
            BecauseOf();
        }

        private void BecauseOf() => exception = Catch.Exception(
            () => templateModelFactory.CreateQuestionnaireExecutorTemplateModel(questionnaire, Create.CodeGenerationSettings()));

        [NUnit.Framework.Test] public void should_not_throw_agrument_exception () =>
            exception.ShouldBeNull();

        private static QuestionnaireExpressionStateModelFactory templateModelFactory;
        private static QuestionnaireDocument questionnaire;
        private static Exception exception;
    }
}