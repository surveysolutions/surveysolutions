using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    internal class when_moving_roster_title_question_to_group_that_have_different_roster_size_question_then_group_of_roster_title_question : QuestionnaireTestsContext
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
            exception.ShouldBeOfType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__move__ = () =>
            exception.Message.ToLower().ShouldContain("move");

        It should_throw_exception_with_message_containting__roster__ = () =>
            exception.Message.ToLower().ShouldContain("roster");

        It should_throw_exception_with_message_containting__title__ = () =>
            exception.Message.ToLower().ShouldContain("title");

        It should_throw_exception_with_message_containting__question__ = () =>
            exception.Message.ToLower().ShouldContain("question");

        It should_throw_exception_with_message_containting__to__ = () =>
            exception.Message.ToLower().ShouldContain("to");

        It should_throw_exception_with_message_containting__group__ = () =>
            exception.Message.ToLower().ShouldContain("group");

        It should_throw_exception_with_message_containting__that__ = () =>
            exception.Message.ToLower().ShouldContain("that");

        It should_throw_exception_with_message_containting__has__ = () =>
            exception.Message.ToLower().ShouldContain("has");

        It should_throw_exception_with_message_containting__size__ = () =>
            exception.Message.ToLower().ShouldContain("size");


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