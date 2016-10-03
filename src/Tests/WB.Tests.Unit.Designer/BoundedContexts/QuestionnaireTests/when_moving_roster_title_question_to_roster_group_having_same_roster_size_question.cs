using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;

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
        };

        Because of =
            () => questionnaire.MoveQuestion(questionId: rosterTitleQuestionId, responsibleId: responsibleId, targetIndex: 0,
                targetGroupId: targetGroupId);


        It should_contains_question = () =>
           questionnaire.QuestionnaireDocument.Find<IQuestion>(rosterTitleQuestionId);

        It should_contains_question_with_PublicKey_equal_to_roster_title_question_id = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(rosterTitleQuestionId)
                .PublicKey.ShouldEqual(rosterTitleQuestionId);

        It should_contains_question_with_GroupKey_equal_to_new_group_id_for_roster_title_question = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(rosterTitleQuestionId)
                .GetParent().PublicKey.ShouldEqual(targetGroupId);

        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid rosterGroupWithRosterTitleQuestionId;
        private static Guid targetGroupId;
        private static Guid chapterId;
        private static Guid rosterSizeQuestionId;
        private static Guid rosterTitleQuestionId;
    }
}