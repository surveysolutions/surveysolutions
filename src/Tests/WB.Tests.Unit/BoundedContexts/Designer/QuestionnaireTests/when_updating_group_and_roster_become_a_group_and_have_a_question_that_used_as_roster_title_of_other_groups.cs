using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_updating_group_and_roster_become_a_group_and_have_a_question_that_used_as_roster_title_of_other_groups :
        QuestionnaireTestsContext
    {
        private Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var anotherRosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");
            rosterTitleQuestionId = Guid.Parse("22222222222222222222222222222222");

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });

            questionnaire.Apply(new NumericQuestionAdded
            {
                PublicKey = rosterSizeQuestionId,
                IsInteger = true,
                GroupPublicKey = chapterId
            });

            AddGroup(questionnaire: questionnaire, groupId: groupId, parentGroupId: chapterId, condition: null, responsibleId: responsibleId,
                rosterSizeQuestionId: rosterSizeQuestionId, isRoster: true, rosterSizeSource: RosterSizeSourceType.Question,
                rosterFixedTitles: null, rosterTitleQuestionId: null);
            
            questionnaire.Apply(new NewQuestionAdded
            {
                PublicKey = rosterTitleQuestionId,
                GroupPublicKey = groupId,
                QuestionType = QuestionType.Text
            });

            AddGroup(questionnaire: questionnaire, groupId: anotherRosterId, parentGroupId: chapterId, condition: null,
                responsibleId: responsibleId, rosterSizeQuestionId: rosterSizeQuestionId, isRoster: true,
                rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: rosterTitleQuestionId);
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.UpdateGroup(groupId: groupId, responsibleId: responsibleId, title: "title", variableName: null, rosterSizeQuestionId: null,
                    description: null, condition: null, isRoster: false, rosterSizeSource: RosterSizeSourceType.Question,
                    rosterFixedTitles: null, rosterTitleQuestionId: null));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__roster__ = () =>
            exception.Message.ToLower().ShouldContain("roster");

        It should_throw_exception_with_message_containting__become__ = () =>
            exception.Message.ToLower().ShouldContain("become");

        It should_throw_exception_with_message_containting__group__ = () =>
            exception.Message.ToLower().ShouldContain("sub-section");

        It should_throw_exception_with_message_containting__contains__ = () =>
            exception.Message.ToLower().ShouldContain("contains");

        It should_throw_exception_with_message_containting__title__ = () =>
            exception.Message.ToLower().ShouldContain("title");

        It should_throw_exception_with_message_containting__question__ = () =>
            exception.Message.ToLower().ShouldContain("question");

        private static Exception exception;
        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid groupId;
        private static Guid rosterSizeQuestionId;
        private static Guid rosterTitleQuestionId;
    }
}