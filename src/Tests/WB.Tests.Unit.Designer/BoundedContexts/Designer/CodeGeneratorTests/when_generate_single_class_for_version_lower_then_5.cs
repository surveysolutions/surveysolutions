using System;
using System.Data;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.CodeGeneratorTests
{
    internal class when_generate_single_class_for_version_lower_then_5 : CodeGeneratorTestsContext
    {
        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocument();
            generator = Create.CodeGenerator();
        };

        Because of = () =>
            exception = Catch.Exception(() => generator.Generate(questionnaire,version));

        It should_throw_VersionNotFoundException = () =>
            exception.ShouldBeOfExactType<VersionNotFoundException>();
        
        private static Version version=new Version(4,0,0);
        private static CodeGenerator generator;
        private static QuestionnaireDocument questionnaire;
        private static Exception exception;
    }
}
