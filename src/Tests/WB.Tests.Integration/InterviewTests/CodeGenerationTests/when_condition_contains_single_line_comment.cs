using Main.Core.Documents;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.CodeGenerationTests
{
    internal class when_condition_contains_single_line_comment : CodeGenerationTestsContext
    {
        [Test]
        public void should_compile()
        {
            var expressionProcessorGenerator = CreateExpressionProcessorGenerator();

            QuestionnaireDocument questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children:
                Create.Entity.NumericIntegerQuestion(enablementCondition: "true // comment"));

            GenerationResult emitResult = 
                expressionProcessorGenerator.GenerateProcessorStateAssembly(questionnaireDocument, LatestQuestionnaireVersion(), 
                    out _);

            Assert.That(emitResult, Has.Property(nameof(emitResult.Success)).True);
        }
    }
}
