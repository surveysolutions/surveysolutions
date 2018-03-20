using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.CodeGeneratorTests
{
    internal class when_creating_models_with_fixed_roster : CodeGeneratorTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            AssemblyContext.SetupServiceLocator();

            questionnaire = Create.QuestionnaireDocument(questionnaireId, children: new[]
            {
                Create.Chapter(children: new IComposite[]
                {
                    Create.Roster(rosterId: Id.gA, variable: "fixed_roster", rosterType: RosterSizeSourceType.FixedTitles, fixedTitles: new string[] {"1", "2"}, enablementCondition: "roster condition", children: new IComposite[]
                    {
                    })
                })
            });

            templateModelFactory = Create.QuestionnaireExecutorTemplateModelFactory();
            BecauseOf();
        }

        private void BecauseOf() =>
            model = templateModelFactory.CreateQuestionnaireExecutorTemplateModel(questionnaire, Create.CodeGenerationSettings());

        [NUnit.Framework.Test] public void should_create_model_with_1_roster () =>
            model.AllRosters.Count.Should().Be(1);

        [NUnit.Framework.Test] public void should_create_questionnaire_level_with_1_question () =>
            model.QuestionnaireLevelModel.Rosters.Count.Should().Be(1);

        [NUnit.Framework.Test] public void should_create_fixed_roster_model () 
        {
            RosterTemplateModel roster = model.AllRosters.Single(x => x.Id == Id.gA);
            roster.Conditions.Should().Be("roster condition");
            roster.VariableName.Should().Be("fixed_roster");
            roster.RosterScopeName.Should().Be("@__fixed_roster_scope");
            roster.TypeName.Should().StartWith("@__fixed_roster_aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa_");
            roster.ParentScopeTypeName.Should().Be(CodeGenerator.QuestionnaireTypeName);
            roster.ParentTypeName.Should().Be(CodeGenerator.QuestionnaireTypeName);
            roster.Questions.Count.Should().Be(0);
            roster.Groups.Count.Should().Be(0);
            roster.Rosters.Count.Should().Be(0);
            roster.RosterScope.Count.Should().Be(1);
        }

        private static QuestionnaireExpressionStateModelFactory templateModelFactory;
        private static QuestionnaireExpressionStateModel model;
        private static QuestionnaireDocument questionnaire;
        private static readonly Guid questionnaireId = Guid.Parse("ffffffffffffffffffffffffffffffff");
    }
}
