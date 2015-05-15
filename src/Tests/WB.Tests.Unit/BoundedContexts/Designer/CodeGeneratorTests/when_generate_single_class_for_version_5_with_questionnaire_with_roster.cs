using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Unit.BoundedContexts.Designer.CodeGeneratorTests
{
    internal class when_generate_single_class_for_version_5_with_questionnaire_with_roster : CodeGeneratorTestsContext
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
                                    rosterSizeSourceType: RosterSizeSourceType.FixedTitles,
                                    fixedTitles: new string[] {"1", "2"})
                            })
                    });

            generator = CreateCodeGenerator();
        };

        Because of = () =>
            generatedClassContent =
                generator.Generate(questionnaire, version);

        It should_generate_class_with_V2_namespaces_included = () =>
            generatedClassContent.ShouldNotContain("WB.Core.SharedKernels.DataCollection.V2");

        It should_generate_class_with_method_UpdateRosterTitle = () =>
            generatedClassContent.ShouldNotContain("UpdateRosterTitle");

        It should_generate_class_with_RosterRowList_as_roster_type = () =>
            generatedClassContent.ShouldNotContain("RosterRowList");

        private static Version version = new Version(5, 0, 0);
        private static CodeGenerator generator;
        private static string generatedClassContent;
        private static readonly Guid chapterId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid rosterId = Guid.Parse("22222222222222222222222222222222");
        private static QuestionnaireDocument questionnaire;
    }
}
