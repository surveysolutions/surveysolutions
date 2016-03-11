using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Tests.Unit.BoundedContexts.Designer.CodeGeneratorTests
{
    internal class when_creating_models_with_fixed_roster : CodeGeneratorTestsContext
    {
        Establish context = () =>
        {
            AssemblyContext.SetupServiceLocator();

            questionnaire = Create.QuestionnaireDocument(questionnaireId, children: new[]
            {
                Create.Chapter(children: new IComposite[]
                {
                    Create.Roster(rosterId: Id.gA, variable: "fixed_roster", rosterSizeSourceType: RosterSizeSourceType.FixedTitles, fixedTitles: new string[] {"1", "2"}, enablementCondition: "roster condition", children: new IComposite[]
                    {
                    })
                })
            });

            templateModelFactory = Create.QuestionnaireExecutorTemplateModelFactory();
        };

        Because of = () =>
            model = templateModelFactory.CreateQuestionnaireExecutorTemplateModel(questionnaire, Create.CodeGenerationSettings());

        It should_create_model_with_1_roster = () =>
            model.AllRosters.Count.ShouldEqual(1);

        It should_create_questionnaire_level_with_1_question = () =>
            model.QuestionnaireLevelModel.Rosters.Count.ShouldEqual(1);

        It should_create_fixed_roster_model = () =>
        {
            RosterTemplateModel roster = model.AllRosters.Single(x => x.Id == Id.gA);
            roster.Conditions.ShouldEqual("roster condition");
            roster.VariableName.ShouldEqual("fixed_roster");
            roster.RosterScopeName.ShouldEqual("@__fixed_roster_scope");
            roster.TypeName.ShouldStartWith("@__fixed_roster_aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa_");
            roster.ParentScopeTypeName.ShouldEqual(CodeGenerator.QuestionnaireTypeName);
            roster.ParentTypeName.ShouldEqual(CodeGenerator.QuestionnaireTypeName);
            roster.Questions.Count.ShouldEqual(0);
            roster.Groups.Count.ShouldEqual(0);
            roster.Rosters.Count.ShouldEqual(0);
            roster.RosterScope.Count.ShouldEqual(1);
        };

        private static QuestionnaireExpressionStateModelFactory templateModelFactory;
        private static QuestionnaireExpressionStateModel model;
        private static QuestionnaireDocument questionnaire;
        private static readonly Guid questionnaireId = Guid.Parse("ffffffffffffffffffffffffffffffff");
    }
}