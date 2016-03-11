using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Tests.Unit.BoundedContexts.Designer.CodeGeneratorTests
{
    internal class when_creating_models_with_multi_roster : CodeGeneratorTestsContext
    {
        Establish context = () =>
        {
            AssemblyContext.SetupServiceLocator();

            questionnaire = Create.QuestionnaireDocument(questionnaireId, children: new[]
            {
                Create.Chapter(children: new IComposite[]
                {
                    Create.MultyOptionsQuestion(Id.g1, variable: "multi", options: new List<Answer>
                    {
                        Create.Option("1", "Option 1"),
                        Create.Option("2", "Option 2")
                    }),
                    Create.Roster(rosterId: Id.gA, variable: "multi_roster", rosterSizeSourceType: RosterSizeSourceType.Question, rosterSizeQuestionId: Id.g1, enablementCondition: "roster condition", children: new IComposite[]
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
            roster.VariableName.ShouldEqual("multi_roster");
            roster.RosterScopeName.ShouldEqual("@__multi_roster_scope");
            roster.TypeName.ShouldStartWith("@__multi_roster_aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa_");
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