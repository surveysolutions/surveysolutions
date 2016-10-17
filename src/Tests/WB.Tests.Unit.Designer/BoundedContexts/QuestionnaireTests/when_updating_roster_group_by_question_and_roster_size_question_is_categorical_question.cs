using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_updating_roster_group_by_question_and_roster_size_question_is_categorical_question : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");
            rosterSizeSourceType = RosterSizeSourceType.Question;

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);

            questionnaire.AddMultiOptionQuestion(rosterSizeQuestionId,chapterId,responsibleId);
            questionnaire.AddGroup(groupId, responsibleId: responsibleId);
        };

        Because of = () =>
            questionnaire.UpdateGroup(groupId, responsibleId, "title",null, rosterSizeQuestionId, null, null, hideIfDisabled: false, isRoster: true,
                rosterSizeSource: rosterSizeSourceType, rosterFixedTitles: null, rosterTitleQuestionId: null);



        It should_contains_group = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId).ShouldNotBeNull();

        It should_contains_group_with_GroupId_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .PublicKey.ShouldEqual(groupId);

        It should_contains_group_with_RosterSizeSourceType_equal_to_specified_rosterSizeSourceType = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .RosterSizeSource.ShouldEqual(rosterSizeSourceType);

        It should_contains_group_with_RosterSizeQuestionId_equal_to_specified_question_id = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .RosterSizeQuestionId.ShouldEqual(rosterSizeQuestionId);

        It should_contains_group_with_FixedRosterTitles_count_should_equal_0 = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId).FixedRosterTitles.Length.ShouldEqual(0);

        It should_contains_group_with_RosterTitleQuestionId_equal_to_null = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId).RosterTitleQuestionId.ShouldBeNull();


        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid groupId;
        private static Guid rosterSizeQuestionId;
        private static RosterSizeSourceType rosterSizeSourceType;
    }
}