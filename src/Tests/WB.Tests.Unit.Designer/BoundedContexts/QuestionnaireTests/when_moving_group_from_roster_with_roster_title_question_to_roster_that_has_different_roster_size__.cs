using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_moving_group_from_roster_with_roster_title_question_to_roster_that_has_different_roster_size_question_than_source_group : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddNumericQuestion(
                rosterSizeQuestion1Id,
                chapterId,
                responsibleId,
                isInteger : true);

            questionnaire.AddNumericQuestion(
                rosterSizeQuestion2Id,
                chapterId,
                responsibleId,
                isInteger : true);
            questionnaire.AddGroup(targetGroupId, chapterId, responsibleId: responsibleId);
            questionnaire.MarkGroupAsRoster(new GroupBecameARoster(responsibleId, targetGroupId));
            questionnaire.ChangeRoster(new RosterChanged(responsibleId: responsibleId, groupId: targetGroupId){
                    RosterSizeQuestionId = rosterSizeQuestion2Id,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    FixedRosterTitles = null,
                    RosterTitleQuestionId = null
                });

            questionnaire.AddGroup(sourceRosterId,  chapterId, responsibleId: responsibleId);
            questionnaire.MarkGroupAsRoster(new GroupBecameARoster(responsibleId, sourceRosterId));
            questionnaire.ChangeRoster(new RosterChanged(responsibleId: responsibleId, groupId: sourceRosterId){
                    RosterSizeQuestionId = rosterSizeQuestion1Id,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    FixedRosterTitles = null,
                    RosterTitleQuestionId = rosterTitleQuestionId
                });

            questionnaire.AddGroup(groupFromRosterId, sourceRosterId, responsibleId: responsibleId);
            questionnaire.AddNumericQuestion(rosterTitleQuestionId,groupFromRosterId,
                responsibleId);
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