using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using Main.Core.Documents;

namespace WB.Tests.Unit.BoundedContexts.Designer.CodeGeneratorTests
{
    internal class when_generate_single_class_for_version_lower_then_5 : CodeGeneratorTestsContext
    {
        Establish context = () =>
        {
            questionnaire=new QuestionnaireDocument();
            generator = CreateCodeGenerator();
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                generator.Generate(questionnaire,version));

        It should_throw_VersionNotFoundException = () =>
            exception.ShouldBeOfExactType<VersionNotFoundException>();
        
        private static Version version=new Version(4,0,0);
        private static CodeGenerator generator;
        private static QuestionnaireDocument questionnaire;
        private static Exception exception;
    }
}
