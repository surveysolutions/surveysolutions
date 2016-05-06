﻿using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.CodeGeneratorTests
{
    internal class when_generate_single_class_for_version_15_with_variables : CodeGeneratorTestsContext
    {
        Establish context = () =>
        {
            questionnaire =
                Create.QuestionnaireDocument();

            generator = Create.CodeGenerator();
        };

        Because of = () =>
            generatedClassContent = generator.Generate(questionnaire, version).Values.First();

        It should_generate_class_with_V9_namespaces_included = () =>
            generatedClassContent.ShouldContain("WB.Core.SharedKernels.DataCollection.V9");

        It should_generate_class_with_AbstractConditionalLevelInstanceV9 = () =>
            generatedClassContent.ShouldContain("AbstractConditionalLevelInstanceV9");

        private static Version version = new Version(15, 0, 0);
        private static CodeGenerator generator;
        private static string generatedClassContent;
        private static readonly Guid chapterId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid rosterId = Guid.Parse("22222222222222222222222222222222");
        private static QuestionnaireDocument questionnaire;
    }
}