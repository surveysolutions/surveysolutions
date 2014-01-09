using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    internal class when_moving_roster_size_question_to_roster_group : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            rosterGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            targetRosterGroupId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");

            rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });

            questionnaire.Apply(new NumericQuestionAdded
            {
                PublicKey = rosterSizeQuestionId,
                IsInteger = true,
                GroupPublicKey = chapterId,
            });

            AddGroup(questionnaire: questionnaire, groupId: rosterGroupId, parentGroupId: chapterId, condition: null,
                responsibleId: responsibleId, rosterSizeQuestionId: rosterSizeQuestionId, isRoster: true);

            AddGroup(questionnaire: questionnaire, groupId: targetRosterGroupId, parentGroupId: chapterId, condition: null,
                responsibleId: responsibleId, rosterSizeQuestionId: null, isRoster: true, rosterSizeSource: RosterSizeSourceType.FixedTitles,
                rosterTitleQuestionId: null, rosterFixedTitles: new[] { "fixed title 1" });
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.MoveQuestion(rosterSizeQuestionId, targetRosterGroupId, targetIndex: 0, responsibleId: responsibleId));
        
        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__title__ = () =>
            exception.Message.ToLower().ShouldContain("size");

        It should_throw_exception_with_message_containting__question__ = () =>
            exception.Message.ToLower().ShouldContain("question");

        It should_throw_exception_with_message_containting__roster__ = () =>
            exception.Message.ToLower().ShouldContain("roster");

        private static Exception exception;
        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid rosterGroupId;
        private static Guid targetRosterGroupId;
        private static Guid chapterId;
        private static Guid rosterSizeQuestionId;
    }
}