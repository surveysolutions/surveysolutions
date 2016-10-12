using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_moving_group_from_roster_with_roster_title_question_to_group_in_roster_by_roster_size_question : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddNumericQuestion(
                rosterSizeQuestionId,chapterId, responsibleId, isInteger : true);
            questionnaire.AddGroup(targetRosterId, chapterId, responsibleId: responsibleId, isRoster: true, rosterSourceType: RosterSizeSourceType.Question,
                rosterSizeQuestionId: rosterSizeQuestionId, rosterFixedTitles: null);
            questionnaire.AddGroup(groupFromTargetRosterId,  targetRosterId, responsibleId: responsibleId);

            questionnaire.AddGroup(sourceRosterId, chapterId, responsibleId: responsibleId, isRoster: true, rosterSourceType: RosterSizeSourceType.Question,
                rosterSizeQuestionId: rosterSizeQuestionId, rosterFixedTitles: null);

            questionnaire.AddGroup(groupFromSourceRosterId, sourceRosterId, responsibleId: responsibleId);
            questionnaire.AddNumericQuestion(rosterTitleQuestionId,groupFromSourceRosterId,responsibleId);

            //update roster title
            /*questionnaire.ChangeRoster(new RosterChanged(responsibleId: responsibleId, groupId: sourceRosterId)
            {
                RosterSizeQuestionId = rosterSizeQuestionId,
                RosterSizeSource = RosterSizeSourceType.Question,
                FixedRosterTitles = null,
                RosterTitleQuestionId = rosterTitleQuestionId
            });*/
        };

        Because of = () => questionnaire.MoveGroup(groupFromSourceRosterId, groupFromTargetRosterId, 0, responsibleId);


        It should_contains_group = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupFromSourceRosterId).ShouldNotBeNull();

        It should_contains_group_with_GroupId_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupFromSourceRosterId)
                .PublicKey.ShouldEqual(groupFromSourceRosterId);

        It should_contains_group_with_ParentGroupId_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupFromSourceRosterId)
                .GetParent().PublicKey.ShouldEqual(groupFromTargetRosterId);

        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static Guid targetRosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid sourceRosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid groupFromSourceRosterId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        private static Guid groupFromTargetRosterId = Guid.Parse("22222222222222222222222222222222");
        private static Guid rosterTitleQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
    }
}