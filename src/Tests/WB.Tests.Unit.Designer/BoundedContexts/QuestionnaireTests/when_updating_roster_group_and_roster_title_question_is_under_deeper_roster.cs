using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_updating_roster_group_and_roster_title_question_is_under_deeper_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            titleQuestionId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");

            var nestedRosterId = Guid.Parse("21111111111111111111111111111111");
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.AddQuestion(Create.Event.NumericQuestionAdded(publicKey : rosterSizeQuestionId, isInteger : true, groupPublicKey : chapterId ));

            questionnaire.AddGroup(new NewGroupAdded { PublicKey = rosterId, ParentGroupPublicKey = chapterId });
            questionnaire.MarkGroupAsRoster(new GroupBecameARoster(responsibleId, rosterId));

            questionnaire.AddGroup(new NewGroupAdded { PublicKey = nestedRosterId, ParentGroupPublicKey = rosterId });
            questionnaire.MarkGroupAsRoster(new GroupBecameARoster(responsibleId, nestedRosterId));

            questionnaire.AddQuestion(Create.Event.NumericQuestionAdded( publicKey : titleQuestionId, isInteger : true, groupPublicKey : nestedRosterId ));
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.UpdateGroup(rosterId, responsibleId, "title", null, rosterSizeQuestionId, null, null, false, true,
                    RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: titleQuestionId));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__question_placed_deeper_then_roster = () =>
            new[] { "question for roster titles", "should be placed only inside groups where roster source question is" }.ShouldEachConformTo(keyword => exception.Message.ToLower().Contains(keyword));

        private static Exception exception;
        private static Guid responsibleId;
        private static Guid rosterId;
        private static Guid rosterSizeQuestionId;
        private static Questionnaire questionnaire;
        private static Guid chapterId;
        private static Guid titleQuestionId;
    }
}
