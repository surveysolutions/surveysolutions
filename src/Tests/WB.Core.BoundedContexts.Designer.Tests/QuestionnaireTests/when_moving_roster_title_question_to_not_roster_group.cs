using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    internal class when_moving_roster_title_question_to_group_that_have_different_roster_size_question_than_group_of_roster_title_question : QuestionnaireTestsContext
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
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });

            questionnaire.Apply(new NumericQuestionAdded
            {
                PublicKey = rosterSizeQuestionId,
                IsInteger = true,
                GroupPublicKey = chapterId,
            });

            AddGroup(questionnaire: questionnaire, groupId: rosterGroupWithRosterTitleQuestionId, parentGroupId: chapterId, condition: null,
                responsibleId: responsibleId, rosterSizeQuestionId: rosterSizeQuestionId, isRoster: true);

            questionnaire.Apply(new NewQuestionAdded
            {
                PublicKey = rosterTitleQuestionId,
                QuestionType = QuestionType.Text,
                GroupPublicKey = rosterGroupWithRosterTitleQuestionId
            });

            AddGroup(questionnaire: questionnaire, groupId: Guid.NewGuid(), parentGroupId: chapterId, condition: null,
                responsibleId: responsibleId, rosterSizeQuestionId: rosterSizeQuestionId, isRoster: true,
                rosterTitleQuestionId: rosterTitleQuestionId);

            AddGroup(questionnaire: questionnaire, groupId: targetGroupId, parentGroupId: chapterId, condition: null,
                responsibleId: responsibleId);
        };

        Because of = () =>
            exception =
                Catch.Exception(
                    () =>
                        questionnaire.MoveQuestion(questionId: rosterTitleQuestionId, responsibleId: responsibleId, targetIndex: 0,
                            targetGroupId: targetGroupId));
        
        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting___can_move___roster_title_question___roster_size_question__ =
            () =>
                new[] { "can move", "roster title question", "roster source question" }.ShouldEachConformTo(
                    keyword => exception.Message.ToLower().Contains(keyword));


        private static Exception exception;
        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid rosterGroupWithRosterTitleQuestionId;
        private static Guid targetGroupId;
        private static Guid chapterId;
        private static Guid rosterSizeQuestionId;
        private static Guid rosterTitleQuestionId;
    }
}