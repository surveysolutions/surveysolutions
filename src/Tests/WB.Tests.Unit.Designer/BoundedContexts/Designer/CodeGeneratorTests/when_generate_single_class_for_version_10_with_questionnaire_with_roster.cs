using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.CodeGeneratorTests
{
    internal class when_generate_single_class_for_version_10_with_questionnaire_with_roster : CodeGeneratorTestsContext
    {
        Establish context = () =>
        {
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
        };

        Because of = () =>
            generatedClassContent = generator.Generate(questionnaire, version).Values.First();

        It should_generate_class_with_V4_namespaces_included = () =>
            generatedClassContent.ShouldContain("WB.Core.SharedKernels.DataCollection.V4");

        It should_generate_class_with_method_IInterviewExpressionStateV4 = () =>
            generatedClassContent.ShouldContain("IInterviewExpressionStateV4");

        It should_generate_class_with_AbstractConditionalLevelInstanceV4 = () =>
            generatedClassContent.ShouldContain("AbstractConditionalLevelInstanceV4");

        private static int version = 10;
        private static CodeGenerator generator;
        private static string generatedClassContent;
        private static readonly Guid chapterId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid rosterId = Guid.Parse("22222222222222222222222222222222");
        private static QuestionnaireDocument questionnaire;
    }
}
