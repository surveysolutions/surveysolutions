using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.CodeGeneratorTests
{
    internal class when_generating_classes_with_multiple_validations : CodeGeneratorTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            AssemblyContext.SetupServiceLocator();

            questionnaire = Create.QuestionnaireDocument(questionnaireId, children: new IComposite[]
            {
                Create.Chapter(children: new IComposite[]
                {
                    Create.TextQuestion(questionId: questionWithMultipleValidations, variable: "a", validationConditions:new [] {Create.ValidationCondition("true","test"),Create.ValidationCondition("false","test")})
                }),
            });

            generator = Create.CodeGenerator();
            BecauseOf();
        }


        private void BecauseOf() =>
            generatedClassContent = generator.Generate(questionnaire, version).Values.First();

        [NUnit.Framework.Test] public void should_generate_class_with_V6_namespace_included () =>
            generatedClassContent.ShouldContain("WB.Core.SharedKernels.DataCollection.V6");

        private static int version = 16;
        private static CodeGenerator generator;
        private static string generatedClassContent;
        private static QuestionnaireDocument questionnaire;
        private static readonly Guid questionnaireId = Guid.Parse("ffffffffffffffffffffffffffffffff");
        private static readonly Guid questionWithMultipleValidations = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
    }
}