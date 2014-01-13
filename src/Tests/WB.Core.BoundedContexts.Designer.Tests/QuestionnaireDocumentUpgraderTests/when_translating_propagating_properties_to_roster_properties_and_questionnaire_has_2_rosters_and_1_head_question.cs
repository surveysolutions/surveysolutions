using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireDocumentUpgraderTests
{
    internal class when_translating_propagating_properties_to_roster_properties_and_questionnaire_has_2_rosters_and_1_head_question : QuestionnaireDocumentUpgraderTestsContext
    {
        Establish context = () =>
        {
            rosterGroupId = Guid.Parse("11111111111111111111111111111111");
            rosterGroup1Id = Guid.Parse("A1111111111111111111111111111111");

            headQuestionId = Guid.Parse("9999999999999999AAAAAAAAAAAAAAAA");

            initialDocument = CreateQuestionnaireDocument(
                CreateAutoPropagatingQuestion(triggers: new List<Guid> { rosterGroupId, rosterGroup1Id }),
                CreateGroup(groupId: rosterGroupId, 
                            children: new[] { CreateHeadQuestion(questionId: headQuestionId) }, 
                            setup: g =>
                            {
                                g.Propagated = Propagate.AutoPropagated;
                            }),
                CreateGroup(groupId: rosterGroup1Id,
                            setup: g =>
                            {
                                g.Propagated = Propagate.AutoPropagated;
                            })
               );
            
            upgrader = CreateQuestionnaireDocumentUpgrader();
        };

        Because of = () =>
            resultDocument = upgrader.TranslatePropagatePropertiesToRosterProperties(initialDocument);

        
        It should_make_referenced_group_roster = () =>
            resultDocument.GetGroup(rosterGroupId).IsRoster.ShouldBeTrue();

        It should_set_roster_title_qestionId_of_parent_group_with_old_head_question_id = () =>
            resultDocument.GetGroup(rosterGroupId).RosterTitleQuestionId.ShouldEqual(headQuestionId);

        It should_set_roster_title_qestionId_of_second_group_with_old_head_question_id = () =>
            resultDocument.GetGroup(rosterGroup1Id).RosterTitleQuestionId.ShouldEqual(headQuestionId);

        It should_set_capital_property_of_old_head_question_to_false = () =>
        resultDocument.GetQuestion<TextQuestion>(headQuestionId).Capital.ShouldBeFalse();


        
        private static QuestionnaireDocument resultDocument;
        private static QuestionnaireDocumentUpgrader upgrader;
        private static QuestionnaireDocument initialDocument;
        private static Guid rosterGroupId;
        private static Guid rosterGroup1Id;
        private static Guid headQuestionId;

    }
}
