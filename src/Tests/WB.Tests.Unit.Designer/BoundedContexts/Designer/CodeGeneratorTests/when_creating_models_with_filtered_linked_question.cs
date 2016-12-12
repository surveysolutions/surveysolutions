using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.CodeGeneratorTests
{
    internal class when_creating_models_with_filtered_linked_question : CodeGeneratorTestsContext
    {
        Establish context = () =>
        {
            AssemblyContext.SetupServiceLocator();

            questionnaire = Create.QuestionnaireDocument(questionnaireId, children: new[]
            {
                Create.Chapter(children: new IComposite[]
                {
                    Create.Roster(rosterId: Id.gA, variable: "fixed_roster", rosterType: RosterSizeSourceType.FixedTitles, fixedTitles: new string[] {"1", "2"}),
                    Create.SingleOptionQuestion(questionId: Id.gB, variable: "a", linkedToRosterId: Id.gA),

                })
            });

            templateModelFactory = Create.QuestionnaireExecutorTemplateModelFactory();
        };

        Because of = () =>
            model = templateModelFactory.CreateQuestionnaireExecutorTemplateModel(questionnaire, Create.CodeGenerationSettings());

        It should_create_roster_model_with_1_LinkedQuestionFilterExpressions = () =>
            model.AllRosters[0].LinkedQuestionFilterExpressions.Count.ShouldEqual(1);

        It should_LinkedQuestionFilterExpressions_where_LinkedQuestionId_equals_to_gB = () =>
            model.AllRosters[0].LinkedQuestionFilterExpressions[0].LinkedQuestionId.ShouldEqual(Id.gB);

        private static QuestionnaireExpressionStateModelFactory templateModelFactory;
        private static QuestionnaireExpressionStateModel model;
        private static QuestionnaireDocument questionnaire;
        private static readonly Guid questionnaireId = Guid.Parse("ffffffffffffffffffffffffffffffff");
    }
}