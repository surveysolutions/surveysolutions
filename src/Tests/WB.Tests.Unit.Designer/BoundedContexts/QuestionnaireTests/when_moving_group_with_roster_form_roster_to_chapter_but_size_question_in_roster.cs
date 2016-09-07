using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_moving_group_with_roster_form_roster_to_chapter_but_size_question_in_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = groupId, ParentGroupPublicKey = chapterId });

            questionnaire.AddGroup(new NewGroupAdded { PublicKey = roster1Id, ParentGroupPublicKey = groupId });
            questionnaire.MarkGroupAsRoster(new GroupBecameARoster(responsibleId, roster1Id));

            questionnaire.AddQuestion(Create.Event.NumericQuestionAdded (publicKey : rosterSizeQuestionId, isInteger : true, groupPublicKey : roster1Id ));

            questionnaire.AddGroup(new NewGroupAdded { PublicKey = roster2Id, ParentGroupPublicKey = roster1Id });
            questionnaire.MarkGroupAsRoster(new GroupBecameARoster(responsibleId, roster2Id));
            questionnaire.ChangeRoster(new RosterChanged(responsibleId, roster2Id)
            {
                RosterSizeQuestionId = rosterSizeQuestionId,
                RosterSizeSource = RosterSizeSourceType.Question,
                FixedRosterTitles = null,
                RosterTitleQuestionId = null
            });
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.MoveGroup(roster2Id, groupId, 0, responsibleId));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__question_placed_deeper_then_roster = () =>
            new[] { "roster", "question", "deeper" }.ShouldEachConformTo(keyword => exception.Message.ToLower().Contains(keyword));

        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid roster1Id = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid roster2Id = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static Exception exception;

    }
}
