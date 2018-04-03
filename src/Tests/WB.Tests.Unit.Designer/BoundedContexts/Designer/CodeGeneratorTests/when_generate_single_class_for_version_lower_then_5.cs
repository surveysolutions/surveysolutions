using System;
using System.Data;
using Main.Core.Documents;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.CodeGeneratorTests
{
    internal class when_generate_single_class_for_version_lower_then_5 : CodeGeneratorTestsContext
    {
        [NUnit.Framework.Test] public void should_throw_VersionNotFoundException () {
            questionnaire = Create.QuestionnaireDocument();
            generator = Create.CodeGenerator();

            Assert.Throws<VersionNotFoundException>(() => generator.Generate(questionnaire,version));
        }

        private static int version = 4;
        private static CodeGenerator generator;
        private static QuestionnaireDocument questionnaire;
    }
}
