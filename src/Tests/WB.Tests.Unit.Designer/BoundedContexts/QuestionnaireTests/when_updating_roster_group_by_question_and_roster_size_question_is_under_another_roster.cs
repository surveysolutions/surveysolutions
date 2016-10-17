using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_updating_roster_group_by_question_and_roster_size_question_is_under_another_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var anotherRosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddGroup(anotherRosterId, responsibleId: responsibleId, isRoster: true);
            
            questionnaire.AddNumericQuestion(rosterSizeQuestionId, isInteger : true, parentId: anotherRosterId , responsibleId:responsibleId);
            questionnaire.AddGroup(groupId, anotherRosterId, responsibleId: responsibleId);
        };


        Because of = () =>
                questionnaire.UpdateGroup(groupId, responsibleId, "title",null, rosterSizeQuestionId, null, null, hideIfDisabled: false, isRoster: true,
                    rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);

        It should_raise_GroupBecameARoster_event = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId);

        It should_raise_GroupBecameARoster_event_with_GroupId_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .PublicKey.ShouldEqual(groupId);

        It should_raise_RosterChanged_event_with_GroupId_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .IsRoster.ShouldBeTrue();


        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid groupId;
        private static Guid rosterSizeQuestionId;
    }
}