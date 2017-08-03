using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.CodeGeneratorTests
{
    internal class when_generating_classes_with_filtered_linked_question : CodeGeneratorTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            AssemblyContext.SetupServiceLocator();
            var assetsTitles = new[]
            {
                Create.FixedRosterTitle(1, "TV"),
                Create.FixedRosterTitle(2, "Microwave"),
                Create.FixedRosterTitle(3, "Cleaner")
            };

            questionnaire = Create.QuestionnaireDocument(questionnaireId, children: new IComposite[]
            {
                Create.Chapter(children: new IComposite[]
                {
                    Create.SingleOptionQuestion(questionId: linkedQuestion, variable: "a", linkedToRosterId: rosterId),
                    Create.Roster(rosterId, variable: "assets", rosterType: RosterSizeSourceType.FixedTitles, fixedRosterTitles: assetsTitles)
                }),
            });

            generator = Create.CodeGenerator();
            BecauseOf();
        }

        private void BecauseOf() =>
            generatedClassContent = generator.Generate(questionnaire, version).Values.First();

        [NUnit.Framework.Test] public void should_generate_class_with_V7_namespace_included () =>
             generatedClassContent.ShouldContain("WB.Core.SharedKernels.DataCollection.V7");

        private static int version = 16;
        private static CodeGenerator generator;
        private static string generatedClassContent;
        private static QuestionnaireDocument questionnaire;
        private static readonly Guid questionnaireId = Guid.Parse("ffffffffffffffffffffffffffffffff");
        private static readonly Guid linkedQuestion = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        private static readonly Guid rosterId = Guid.Parse("cccccccccccccccccccccccccccccccc");
    }
}