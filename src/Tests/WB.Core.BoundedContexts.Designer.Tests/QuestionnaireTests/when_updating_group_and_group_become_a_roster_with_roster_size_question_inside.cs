using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    internal class when_updating_group_and_group_become_a_roster_with_roster_size_question_inside : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NewGroupAdded { PublicKey = groupId, ParentGroupPublicKey = chapterId });
            questionnaire.Apply(new NumericQuestionAdded()
            {
                PublicKey = rosterSizeQuestionId,
                GroupPublicKey = groupId,
                IsInteger = true,
                MaxAllowedValue = 5
            });
            
            questionnaire.Apply(new NewGroupAdded { PublicKey = rosterId, ParentGroupPublicKey = chapterId });
            questionnaire.Apply(new GroupBecameARoster(responsibleId, rosterId));
            questionnaire.Apply(new RosterChanged(responsibleId: responsibleId, groupId: rosterId,
                rosterSizeQuestionId: rosterSizeQuestionId, rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null,
                rosterTitleQuestionId: null));
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.UpdateGroup(groupId: groupId, responsibleId: responsibleId, title: "title", rosterSizeQuestionId: rosterSizeQuestionId,
                    description: null, condition: null, isRoster: true, rosterSizeSource: RosterSizeSourceType.Question,
                    rosterFixedTitles: null, rosterTitleQuestionId: null));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__group____contains____roster_size_question___ =
            () =>
                new[] { "group", "contains", "roster size question" }.ShouldEachConformTo(
                    keyword => exception.Message.ToLower().Contains(keyword));

        private static Exception exception;
        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid rosterId = Guid.Parse("11111111111111111111111111111111");
        private static Guid rosterSizeQuestionId = Guid.Parse("22222222222222222222222222222222");
    }
}