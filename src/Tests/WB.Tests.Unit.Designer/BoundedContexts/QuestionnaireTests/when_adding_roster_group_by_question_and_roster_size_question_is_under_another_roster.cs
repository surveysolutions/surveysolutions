using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_adding_roster_group_by_question_and_roster_size_question_is_under_another_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            anotherRosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddGroup(anotherRosterId, responsibleId: responsibleId, isRoster: true);
           
            questionnaire.AddNumericQuestion(
                rosterSizeQuestionId, isInteger : true, parentId : anotherRosterId,
                responsibleId:responsibleId);
        };


        Because of = () =>
                questionnaire.AddGroupAndMoveIfNeeded(groupId, responsibleId, "title",null, rosterSizeQuestionId, null, null, false, anotherRosterId, true,
                    RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);

        It should_contains_group = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId).ShouldNotBeNull();

        It should_contains_group_with_GroupId_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .PublicKey.ShouldEqual(groupId);

        It should_contains_group_with_ParentGroupId_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .GetParent().PublicKey.ShouldEqual(anotherRosterId);

        It should_contains_group_with_Title_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .Title.ShouldEqual("title");



        private static Guid responsibleId;
        private static Guid groupId;
        private static Guid chapterId;
        private static Guid rosterSizeQuestionId;
        private static Questionnaire questionnaire;
        private static Guid anotherRosterId;
    }
}