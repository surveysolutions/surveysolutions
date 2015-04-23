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
            questionnaire = CreateQuestionnaireDocument(
                new IComposite[]
                {
                    new Group
                    {
                        Title = "Chapter",
                        PublicKey = chapterId,
                        Children = new List<IComposite>
                        {
                            new Group
                            {
                                PublicKey = rosterId,
                                VariableName = "fixed_roster",
                                IsRoster = true,
                                RosterSizeSource = RosterSizeSourceType.FixedTitles,
                                FixedRosterTitles = new[] {new FixedRosterTitle(1, "1"), new FixedRosterTitle(2, "2")}
                            }
                        }
                    },
                });

            generator = CreateCodeGenerator();
        };

        Because of = () =>
            generatedClassContent =
                generator.Generate(questionnaire, version);

        It generated_class_should_not_contain_using_of_V2_namespaces = () =>
            generatedClassContent.ShouldNotContain("WB.Core.SharedKernels.DataCollection.V2");

        It generated_class_should_not_contain_method_UpdateRosterTitle = () =>
            generatedClassContent.ShouldNotContain("UpdateRosterTitle");

        It generated_class_should_not_contain_type_RosterRowList = () =>
            generatedClassContent.ShouldNotContain("RosterRowList");

        private static Version version = new Version(5, 0, 0);
        private static CodeGenerator generator;
        private static string generatedClassContent;
        private static readonly Guid chapterId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid rosterId = Guid.Parse("22222222222222222222222222222222");
        private static QuestionnaireDocument questionnaire;
    }
}
