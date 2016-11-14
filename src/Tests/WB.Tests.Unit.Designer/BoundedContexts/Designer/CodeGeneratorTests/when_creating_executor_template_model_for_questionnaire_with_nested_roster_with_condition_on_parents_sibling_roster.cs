using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.CodeGeneratorTests
{
    internal class when_creating_executor_template_model_for_questionnaire_with_nested_roster_with_condition_on_parents_sibling_roster : CodeGeneratorTestsContext
    {
        Establish context = () =>
        {
            var numericNestedRosterId = Guid.NewGuid();
            questionnaire = CreateQuestionnaireDocument(
                new IComposite[]
                {
                    Create.Group(groupId: Guid.NewGuid(), children: new IComposite[]
                    {
                        Create.NumericIntegerQuestion(id: numericId, variable: "n1"),
                        Create.Roster(rosterId: r1Id, variable: "r1", rosterSizeQuestionId: numericId,
                            rosterType: RosterSizeSourceType.Question,
                            children: new IComposite[]
                            {
                                Create.NumericIntegerQuestion(id: numericNestedRosterId, variable: "r1n1"),
                                Create.Roster(rosterId: r1r1, variable: "r1r1",
                                    rosterType: RosterSizeSourceType.Question,
                                    rosterSizeQuestionId: numericNestedRosterId, children: new IComposite[]
                                    {
                                        Create.TextQuestion(questionId: Guid.NewGuid(), variable: "r1r1t1",
                                            enablementCondition: "r2n1 > 0"),
                                        Create.Roster(rosterId: r1r1r1, variable: "r1r1r1", children: new IComposite[]
                                        {
                                            Create.TextQuestion(questionId: Guid.NewGuid(), variable: "r1r1r1t1",
                                                enablementCondition: "r2n1 > r1n1")
                                        })
                                    })
                            }),
                        Create.Roster(rosterId: r2Id, variable: "r2", rosterSizeQuestionId: numericId,
                            rosterType: RosterSizeSourceType.Question,
                            children: new IComposite[]
                            {
                                Create.NumericIntegerQuestion(id: Guid.NewGuid(), variable: "r2n1")
                            }),
                    })
                });

            expressionStateModelFactory = Create.QuestionnaireExecutorTemplateModelFactory();
        };

        Because of = () =>
            templateModel = expressionStateModelFactory.CreateQuestionnaireExecutorTemplateModel(questionnaire, Create.CodeGenerationSettings());

        It should_variable_r2n1_be_accesible_for_nested_roster = () =>
            GetRosterScopeByRosterId(r1r1).AllParentsQuestionsToTop.Count(x => x.VariableName == "r2n1").ShouldEqual(1);

        It should_variable_r1n1_be_accesible_for_nested_roster = () =>
           GetRosterScopeByRosterId(r1r1).AllParentsQuestionsToTop.Count(x => x.VariableName == "r1n1").ShouldEqual(1);

        It should_variable_r1n1_be_accesible_for_sibling_roster = () =>
           GetRosterScopeByRosterId(r2Id).Questions.Count(x => x.VariableName == "r1n1").ShouldEqual(1);

        It should_variable_r2n1_be_accesible_for_nested_nested_roster = () =>
           GetRosterScopeByRosterId(r1r1r1).AllParentsQuestionsToTop.Count(x => x.VariableName == "r2n1").ShouldEqual(1);

        It should_variable_r1n1_be_accesible_for_nested_nested_roster = () =>
           GetRosterScopeByRosterId(r1r1r1).AllParentsQuestionsToTop.Count(x => x.VariableName == "r1n1").ShouldEqual(1);

        private static RosterScopeTemplateModel GetRosterScopeByRosterId(Guid id)
        {
            return templateModel.RostersGroupedByScope.Single(r => r.Value.RostersInScope.Any(x => x.Id == id)).Value;
        }

        private static QuestionnaireExpressionStateModelFactory expressionStateModelFactory;
        private static QuestionnaireExpressionStateModel templateModel;
        private static QuestionnaireDocument questionnaire;
        private static readonly Guid r2Id = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid r1Id = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid numericId = Guid.Parse("33333333333333333333333333333333");
        private static readonly Guid r1r1 = Guid.Parse("44444444444444444444444444444444");
        private static readonly Guid r1r1r1 = Guid.Parse("55555555555555555555555555555555");
    }
}
