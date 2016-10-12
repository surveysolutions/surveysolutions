using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_adding_roster_group_by_numeric_question_and_roster_title_inside_group_by_roster_size : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            parentGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            groupId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            rosterTitleQuestionId = Guid.Parse("1BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            rosterSizeQuestionId = Guid.Parse("2BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            rosterGroupWithRosterTitleQuestionId = Guid.Parse("3BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            rosterSizeSourceType = RosterSizeSourceType.Question;

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddNumericQuestion(
                rosterSizeQuestionId,
                chapterId,
                responsibleId,
                isInteger: true);
            questionnaire.AddGroup(rosterGroupWithRosterTitleQuestionId, chapterId, responsibleId: responsibleId, isRoster: true, rosterSourceType: RosterSizeSourceType.Question,
                rosterSizeQuestionId: rosterSizeQuestionId, rosterFixedTitles: null);
            
            questionnaire.AddTextQuestion(
                rosterTitleQuestionId,
                rosterGroupWithRosterTitleQuestionId,
                responsibleId);

            //update title
            /*questionnaire.ChangeRoster(new RosterChanged(responsibleId: responsibleId, groupId: rosterGroupWithRosterTitleQuestionId)
            {
                RosterSizeQuestionId = rosterSizeQuestionId,
                RosterSizeSource = RosterSizeSourceType.Question,
                FixedRosterTitles = null,
                RosterTitleQuestionId = rosterTitleQuestionId
            });*/
            
            questionnaire.AddGroup(parentGroupId, responsibleId: responsibleId);
        };

        Because of = () =>
            questionnaire.AddGroupAndMoveIfNeeded(groupId: groupId, responsibleId: responsibleId, title: "title", variableName: null, rosterSizeQuestionId: rosterSizeQuestionId,
                description: null, condition: null, hideIfDisabled: false, parentGroupId: parentGroupId, isRoster: true, rosterSizeSource: rosterSizeSourceType, rosterFixedTitles: null, rosterTitleQuestionId: rosterTitleQuestionId);



        It should_contains_group_with_GroupId_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .PublicKey.ShouldEqual(groupId);

        It should_contains_group_with_RosterSizeQuestionId_equal_to_specified_question_id = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .RosterSizeQuestionId.ShouldEqual(rosterSizeQuestionId);

        It should_contains_group_with_RosterTitleQuestionId_equal_to_specified_question_id = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .RosterTitleQuestionId.ShouldEqual(rosterTitleQuestionId);

        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid groupId;
        private static Guid rosterTitleQuestionId;
        private static Guid rosterSizeQuestionId;
        private static Guid parentGroupId;
        private static Guid rosterGroupWithRosterTitleQuestionId;
        private static RosterSizeSourceType rosterSizeSourceType;
    }
}