using System.Collections.Generic;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.CodeGeneratorTests
{
    internal class when_creating_models_with_question_with_options_filter_and_condition : CodeGeneratorTestsContext
    {
        [Test]
        public void should_not_throw_agrument_exception() {
            AssemblyContext.SetupServiceLocator();

            var questionnaire = Create.QuestionnaireDocument(children: new[]
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

            var templateModelFactory = Create.QuestionnaireExecutorTemplateModelFactory(expressionProcessor: new RoslynExpressionProcessor());

            Assert.DoesNotThrow(() => templateModelFactory.CreateQuestionnaireExecutorTemplateModel(questionnaire, Create.CodeGenerationSettings()));
        }
    }
}