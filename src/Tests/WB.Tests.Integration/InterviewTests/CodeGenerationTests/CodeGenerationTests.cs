using System.Collections.Generic;
using Main.Core.Documents;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.CodeGenerationTests
{
    internal class CodeGenerationTests: CodeGenerationTestsContext
    {

        [Test]
        public void when_geography_questions_is_used()
        {
            var expressionProcessorGenerator = CreateExpressionProcessorGenerator();

            QuestionnaireDocument questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children:
                Create.Entity.GeographyQuestion(variable:"geo",
                    validationConditions:new List<ValidationCondition>()
                    {
                        new ValidationCondition("geo.Length > 0 && geo.Area > 0 && geo.PointsCount > 1", "error")
                    } ));

            GenerationResult emitResult =
                expressionProcessorGenerator.GenerateProcessorStateAssembly(questionnaireDocument, LatestQuestionnaireVersion(),
                    out _);

            Assert.That(emitResult, Has.Property(nameof(emitResult.Success)).True);
        }
    }
}
