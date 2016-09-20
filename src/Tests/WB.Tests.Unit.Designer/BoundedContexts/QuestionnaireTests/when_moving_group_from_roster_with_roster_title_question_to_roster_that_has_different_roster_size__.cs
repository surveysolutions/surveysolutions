using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_moving_group_from_roster_with_roster_title_question_to_roster_that_has_different_roster_size_question_than_source_group : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.AddQuestion(Create.Event.NewQuestionAdded(
                publicKey : rosterSizeQuestion1Id,
                groupPublicKey : chapterId,
                isInteger : true,
                questionType: QuestionType.Numeric
            ));
            questionnaire.AddQuestion(Create.Event.NewQuestionAdded(
                publicKey : rosterSizeQuestion2Id,
                groupPublicKey : chapterId,
                isInteger : true,
                questionType:QuestionType.Numeric
            ));
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = targetGroupId, ParentGroupPublicKey = chapterId});
            questionnaire.MarkGroupAsRoster(new GroupBecameARoster(responsibleId, targetGroupId));
            questionnaire.ChangeRoster(new RosterChanged(responsibleId: responsibleId, groupId: targetGroupId){
                    RosterSizeQuestionId = rosterSizeQuestion2Id,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    FixedRosterTitles = null,
                    RosterTitleQuestionId = null
                });

            questionnaire.AddGroup(new NewGroupAdded { PublicKey = sourceRosterId, ParentGroupPublicKey = chapterId });
            questionnaire.MarkGroupAsRoster(new GroupBecameARoster(responsibleId, sourceRosterId));
            questionnaire.ChangeRoster(new RosterChanged(responsibleId: responsibleId, groupId: sourceRosterId){
                    RosterSizeQuestionId = rosterSizeQuestion1Id,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    FixedRosterTitles = null,
                    RosterTitleQuestionId = rosterTitleQuestionId
                });

            questionnaire.AddGroup(new NewGroupAdded { PublicKey = groupFromRosterId, ParentGroupPublicKey = sourceRosterId });
            questionnaire.AddQuestion(Create.Event.NewQuestionAdded(
                publicKey : rosterTitleQuestionId,
                groupPublicKey : groupFromRosterId,
                questionType:QuestionType.Numeric
            ));
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.MoveGroup(groupFromRosterId, targetGroupId, 0, responsibleId));
        
        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__group____could_not_be_moved____roster_size_question____not_the_same__ =
            () =>
                new[] { "group", "could not be moved", "roster size question", "not the same" }.ShouldEachConformTo(
                    keyword => exception.Message.ToLower().Contains(keyword));

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