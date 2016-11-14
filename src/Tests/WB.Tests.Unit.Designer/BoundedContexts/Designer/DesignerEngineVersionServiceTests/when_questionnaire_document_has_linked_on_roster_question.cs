using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.DesignerEngineVersionServiceTests
{
    internal class when_questionnaire_document_has_linked_on_roster_question
    {
        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocument(questionnaireId,
                Create.Group(groupId: groupId, children: new IComposite[]
                {
                    Create.Roster(rosterId: rosterId, variable: "fixed_roster", rosterType: RosterSizeSourceType.FixedTitles, fixedTitles: new string[] {"1", "2"}, enablementCondition: "roster condition"),
                    Create.SingleQuestion(id: questionId, variable: "linkedToRoster", linkedToRosterId: rosterId)
                }));

            designerEngineVersionService = Create.DesignerEngineVersionService();
        };

        Because of = () =>
            version = designerEngineVersionService.GetQuestionnaireContentVersion(questionnaire);

        It should_return_12_version = () =>
            version.ShouldEqual(12);

        private static int version;
        private static IDesignerEngineVersionService designerEngineVersionService;
        private static QuestionnaireDocument questionnaire;
        private static readonly Guid questionId = Id.g1;
        private static readonly Guid rosterId = Id.g2;
        private static readonly Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid questionnaireId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
    }
}