using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_moving_roster_title_question_to_group_that_have_the_same_roster_size_question_as_in_roster_title_question_group : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            rosterGroupWithRosterTitleQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            targetGroupId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");

            rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");
            rosterTitleQuestionId = Guid.Parse("22222222222222222222222222222222");

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });

            questionnaire.AddQuestion(Create.Event.NumericQuestionAdded
            (
                publicKey : rosterSizeQuestionId,
                isInteger : true,
                groupPublicKey : chapterId
            ));

            AddGroup(questionnaire: questionnaire, groupId: rosterGroupWithRosterTitleQuestionId, parentGroupId: chapterId, condition: null,
                responsibleId: responsibleId, rosterSizeQuestionId: rosterSizeQuestionId, isRoster: true);

            questionnaire.AddQuestion(Create.Event.NewQuestionAdded
            (
                publicKey : rosterTitleQuestionId,
                questionType : QuestionType.Text,
                groupPublicKey : rosterGroupWithRosterTitleQuestionId
            ));

            AddGroup(questionnaire: questionnaire, groupId: Guid.NewGuid(), parentGroupId: chapterId, condition: null,
                responsibleId: responsibleId, rosterSizeQuestionId: rosterSizeQuestionId, isRoster: true,
                rosterTitleQuestionId: rosterTitleQuestionId);

            AddGroup(questionnaire: questionnaire, groupId: targetGroupId, parentGroupId: chapterId, condition: null,
                responsibleId: responsibleId, isRoster: true, rosterSizeQuestionId: rosterSizeQuestionId);

            eventContext = new EventContext();
        };

        Because of =
            () => questionnaire.MoveQuestion(questionId: rosterTitleQuestionId, responsibleId: responsibleId, targetIndex: 0,
                targetGroupId: targetGroupId);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_QuestionnaireItemMoved_event = () =>
           eventContext.ShouldContainEvent<QuestionnaireItemMoved>();

        It should_raise_QuestionnaireItemMoved_event_with_PublicKey_equal_to_roster_title_question_id = () =>
            eventContext.GetSingleEvent<QuestionnaireItemMoved>()
                .PublicKey.ShouldEqual(rosterTitleQuestionId);

        It should_raise_QuestionnaireItemMoved_event_with_GroupKey_equal_to_new_group_id_for_roster_title_question = () =>
            eventContext.GetSingleEvent<QuestionnaireItemMoved>()
                .GroupKey.ShouldEqual(targetGroupId);

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid rosterGroupWithRosterTitleQuestionId;
        private static Guid targetGroupId;
        private static Guid chapterId;
        private static Guid rosterSizeQuestionId;
        private static Guid rosterTitleQuestionId;
    }
}