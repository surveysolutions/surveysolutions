using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    internal class when_moving_group_from_roster_with_roster_title_question_to_roster_that_has_different_roster_size_question_than_source_group : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NumericQuestionAdded()
            {
                PublicKey = rosterSizeQuestion1Id,
                GroupPublicKey = chapterId,
                IsInteger = true,
                MaxAllowedValue = 5
            });
            questionnaire.Apply(new NumericQuestionAdded()
            {
                PublicKey = rosterSizeQuestion2Id,
                GroupPublicKey = chapterId,
                IsInteger = true,
                MaxAllowedValue = 5
            });
            questionnaire.Apply(new NewGroupAdded { PublicKey = targetGroupId, ParentGroupPublicKey = chapterId});
            questionnaire.Apply(new GroupBecameARoster(responsibleId, targetGroupId));
            questionnaire.Apply(new RosterChanged(responsibleId: responsibleId, groupId: targetGroupId,
                rosterTitleQuestionId: null, rosterSizeSource: RosterSizeSourceType.Question,
                rosterSizeQuestionId: rosterSizeQuestion2Id, rosterFixedTitles: null));

            questionnaire.Apply(new NewGroupAdded { PublicKey = sourceRosterId, ParentGroupPublicKey = chapterId });
            questionnaire.Apply(new GroupBecameARoster(responsibleId, sourceRosterId));
            questionnaire.Apply(new RosterChanged(responsibleId: responsibleId, groupId: sourceRosterId,
                rosterTitleQuestionId: rosterTitleQuestionId, rosterSizeSource: RosterSizeSourceType.Question,
                rosterSizeQuestionId: rosterSizeQuestion1Id, rosterFixedTitles: null));

            questionnaire.Apply(new NewGroupAdded { PublicKey = groupFromRosterId, ParentGroupPublicKey = sourceRosterId });
            questionnaire.Apply(new NumericQuestionAdded()
            {
                PublicKey = rosterTitleQuestionId,
                GroupPublicKey = groupFromRosterId
            });
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.MoveGroup(groupFromRosterId, targetGroupId, 0, responsibleId));
        
        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__roster__ = () =>
            exception.Message.ToLower().ShouldContain("roster");

        It should_throw_exception_with_message_containting__title__ = () =>
            exception.Message.ToLower().ShouldContain("title");

        It should_throw_exception_with_message_containting__question__ = () =>
            exception.Message.ToLower().ShouldContain("question");

        It should_throw_exception_with_message_containting__in__ = () =>
            exception.Message.ToLower().ShouldContain("in");

        It should_throw_exception_with_message_containting__group__ = () =>
            exception.Message.ToLower().ShouldContain("group");

        It should_throw_exception_with_message_containting__not__ = () =>
            exception.Message.ToLower().ShouldContain("not");

        It should_throw_exception_with_message_containting__size__ = () =>
            exception.Message.ToLower().ShouldContain("size");

        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static Guid targetGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid sourceRosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid groupFromRosterId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        private static Guid rosterTitleQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid rosterSizeQuestion1Id = Guid.Parse("11111111111111111111111111111111");
        private static Guid rosterSizeQuestion2Id = Guid.Parse("22222222222222222222222222222222");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Exception exception;
    }
}