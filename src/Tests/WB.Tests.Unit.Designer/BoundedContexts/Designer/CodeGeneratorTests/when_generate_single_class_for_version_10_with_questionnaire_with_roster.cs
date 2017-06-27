using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.CodeGeneratorTests
{
    internal class when_generate_single_class_for_version_16_with_questionnaire_with_roster : CodeGeneratorTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire =
                Create.QuestionnaireDocument(
                    children: new[]
                    {
                        Create.Chapter(
                            title: "Chapter",
                            chapterId: chapterId,
                            children: new[]
                            {
                                Create.Roster(
                                    rosterId: rosterId,
                                    variable: "fixed_roster",
                                    rosterType: RosterSizeSourceType.FixedTitles,
                                    fixedTitles: new string[] {"1", "2"})
                            })
                    });

            generator = Create.CodeGenerator();
            BecauseOf();
        }

        private void BecauseOf() =>
            generatedClassContent = generator.Generate(questionnaire, version).Values.First();

        [NUnit.Framework.Test] public void should_generate_class_with_V10_namespaces_included () =>
            generatedClassContent.ShouldContain("WB.Core.SharedKernels.DataCollection.V10");

        [NUnit.Framework.Test] public void should_generate_class_with_AbstractConditionalLevelInstanceV10 () =>
            generatedClassContent.ShouldContain("AbstractConditionalLevelInstanceV10");

        private static int version = 16;
        private static CodeGenerator generator;
        private static string generatedClassContent;
        private static readonly Guid chapterId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid rosterId = Guid.Parse("22222222222222222222222222222222");
        private static QuestionnaireDocument questionnaire;
    }
}
