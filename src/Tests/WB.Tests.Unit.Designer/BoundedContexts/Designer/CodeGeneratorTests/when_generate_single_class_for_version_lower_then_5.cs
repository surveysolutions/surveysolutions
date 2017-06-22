using System;
using System.Data;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.CodeGeneratorTests
{
    internal class when_generate_single_class_for_version_lower_then_5 : CodeGeneratorTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = Create.QuestionnaireDocument();
            generator = Create.CodeGenerator();
            BecauseOf();
        }

        private void BecauseOf() =>
            exception = Catch.Exception(() => generator.Generate(questionnaire,version));

        [NUnit.Framework.Test] public void should_throw_VersionNotFoundException () =>
            exception.ShouldBeOfExactType<VersionNotFoundException>();
        
        private static int version = 4;
        private static CodeGenerator generator;
        private static QuestionnaireDocument questionnaire;
        private static Exception exception;
    }
}
