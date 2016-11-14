using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.DesignerEngineVersionServiceTests
{
    internal class when_questionnaire_contains_linked_question_with_filter_and_uses_current_keyword
    {
        Establish context = () =>
        {
            Guid staticTextId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionnaire = Create.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Roster(rosterId: rosterId, variable: "fixed_roster", rosterType: RosterSizeSourceType.FixedTitles, fixedTitles: new string[] {"1", "2"}),
                Create.SingleQuestion(id: questionId, variable: "linkedToRoster", linkedFilter: "@current.@rowcode != @rowcode", linkedToRosterId: rosterId)
            });
            designerEngineVersionService = Create.DesignerEngineVersionService();
        };

        Because of = () => 
            calculatedVersion = designerEngineVersionService.GetQuestionnaireContentVersion(questionnaire);

        It should_return_version_16 = () => 
            calculatedVersion.ShouldEqual(16);

        static QuestionnaireDocument questionnaire;
        static IDesignerEngineVersionService designerEngineVersionService;
        static int calculatedVersion;
        private static readonly Guid questionId = Id.g1;
        private static readonly Guid rosterId = Id.g2;
    }
}