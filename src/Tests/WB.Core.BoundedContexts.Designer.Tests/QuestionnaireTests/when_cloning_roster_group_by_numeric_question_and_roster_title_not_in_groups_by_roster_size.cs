using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    internal class when_cloning_roster_group_by_numeric_question_and_roster_title_not_in_groups_by_roster_size : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            parentGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            groupId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            rosterTitleQuestionId = Guid.Parse("1BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            rosterSizeQuestionId = Guid.Parse("2BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            rosterSizeSourceType = RosterSizeSourceType.Question;

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NewGroupAdded { PublicKey = groupId, ParentGroupPublicKey = chapterId });
            questionnaire.Apply(new NewQuestionAdded()
            {
                PublicKey = rosterTitleQuestionId,
                GroupPublicKey = chapterId,
                QuestionType = QuestionType.Text
            });
            questionnaire.Apply(new NewQuestionAdded()
            {
                PublicKey = rosterSizeQuestionId,
                GroupPublicKey = chapterId,
                QuestionType = QuestionType.Numeric,
                IsInteger = true
            });
            questionnaire.Apply(new NewGroupAdded { PublicKey = parentGroupId });
        };

        Because of = () =>
            exception = Catch.Exception(
                () =>
                    questionnaire.CloneGroupWithoutChildren(groupId: newGroupId, responsibleId: responsibleId, title: "title",
                        parentGroupId: parentGroupId, description: null, condition: null, rosterSizeQuestionId: rosterSizeQuestionId,
                        isRoster: true, rosterSizeSource: rosterSizeSourceType, rosterFixedTitles: null,
                        rosterTitleQuestionId: rosterTitleQuestionId, targetIndex: 0, sourceGroupId: groupId));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__roster__ = () =>
            exception.Message.ToLower().ShouldContain("roster");

        It should_throw_exception_with_message_containting__title__ = () =>
            exception.Message.ToLower().ShouldContain("title");

        It should_throw_exception_with_message_containting__question__ = () =>
            exception.Message.ToLower().ShouldContain("question");

        It should_throw_exception_with_message_containting__only__ = () =>
            exception.Message.ToLower().ShouldContain("only");

        It should_throw_exception_with_message_containting__by__ = () =>
            exception.Message.ToLower().ShouldContain("by");

        It should_throw_exception_with_message_containting__size__ = () =>
            exception.Message.ToLower().ShouldContain("size");

        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid groupId;
        private static Guid newGroupId;
        private static Guid rosterTitleQuestionId;
        private static Guid rosterSizeQuestionId;
        private static Guid parentGroupId;
        private static RosterSizeSourceType rosterSizeSourceType;
        private static Exception exception;
    }
}