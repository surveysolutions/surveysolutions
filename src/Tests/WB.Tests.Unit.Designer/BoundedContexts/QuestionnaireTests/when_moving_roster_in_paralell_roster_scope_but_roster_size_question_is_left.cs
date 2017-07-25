using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_moving_roster_in_paralell_roster_scope_but_roster_size_question_is_left: QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var rosterSizeQuestionId = Guid.Parse("31111111111111111111111111111111");
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);

            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);

            questionnaire.AddGroup( anotherRosterId,  chapterId, responsibleId: responsibleId, isRoster:true);
            questionnaire.AddGroup(parallelRosterId,  chapterId, responsibleId: responsibleId, isRoster:true);
            
            questionnaire.AddNumericQuestion(rosterSizeQuestionId, isInteger : true, parentId :parallelRosterId , responsibleId:responsibleId);

            questionnaire.AddGroup( groupId, parallelRosterId, responsibleId: responsibleId, isRoster: true, rosterSourceType: RosterSizeSourceType.Question,
                rosterSizeQuestionId: rosterSizeQuestionId, rosterFixedTitles: null);
            BecauseOf();
        }

        private void BecauseOf() =>
            exception = Catch.Exception(() =>
                questionnaire.MoveGroup(groupId, anotherRosterId, 0, responsibleId));

        [NUnit.Framework.Test] public void should_throw_QuestionnaireException () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__question_placed_deeper_then_roster () =>
            new[] { "roster", "question", "deeper" }.ShouldEachConformTo(keyword => exception.Message.ToLower().Contains(keyword));

        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid anotherRosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid parallelRosterId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Exception exception;
    }
}
