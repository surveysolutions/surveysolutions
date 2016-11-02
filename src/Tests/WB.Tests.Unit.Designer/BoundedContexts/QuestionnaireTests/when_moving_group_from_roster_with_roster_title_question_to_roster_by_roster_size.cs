using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_moving_group_from_roster_with_roster_title_question_to_roster_by_roster_size_question : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddNumericQuestion(rosterSizeQuestionId,chapterId,responsibleId, isInteger : true);
            questionnaire.AddGroup(targetGroupId, chapterId, responsibleId: responsibleId, isRoster: true, rosterSourceType: RosterSizeSourceType.Question,
                rosterSizeQuestionId: rosterSizeQuestionId, rosterFixedTitles: null);
            

            questionnaire.AddGroup(sourceRosterId, chapterId, responsibleId: responsibleId, isRoster: true, rosterSourceType: RosterSizeSourceType.Question,
                rosterSizeQuestionId: rosterSizeQuestionId, rosterFixedTitles: null);
            
            questionnaire.AddGroup(groupFromRosterId,  sourceRosterId, responsibleId: responsibleId);
            questionnaire.AddNumericQuestion(rosterTitleQuestionId, groupFromRosterId, responsibleId);
        };

        Because of = () => questionnaire.MoveGroup(groupFromRosterId, targetGroupId, 0, responsibleId);


        It should_contains_group = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupFromRosterId);

        It should_contains_group_with_GroupId_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupFromRosterId)
                .PublicKey.ShouldEqual(groupFromRosterId);

        It should_contains_group_with_ParentGroupId_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupFromRosterId)
                .GetParent().PublicKey.ShouldEqual(targetGroupId);

        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static Guid targetGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid sourceRosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid groupFromRosterId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        private static Guid rosterTitleQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
    }
}