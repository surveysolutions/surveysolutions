using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_updating_roster_group_and_roster_title_question_is_under_deeper_roster : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            titleQuestionId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");

            var nestedRosterId = Guid.Parse("21111111111111111111111111111111");
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddNumericQuestion( rosterSizeQuestionId, isInteger : true, parentId : chapterId , responsibleId:responsibleId);

            questionnaire.AddGroup(rosterId,chapterId, responsibleId: responsibleId, isRoster:true);
            questionnaire.AddGroup(nestedRosterId, rosterId, responsibleId: responsibleId, isRoster:true);

            questionnaire.AddNumericQuestion( titleQuestionId, isInteger : true, parentId: nestedRosterId, responsibleId:responsibleId);
            BecauseOf();
        }

        private void BecauseOf() =>
            exception = Catch.Exception(() =>
                questionnaire.UpdateGroup(rosterId, responsibleId, "title", null, rosterSizeQuestionId, null, null, false, true,
                    RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: titleQuestionId));

        [NUnit.Framework.Test] public void should_throw_QuestionnaireException () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__question_placed_deeper_then_roster () =>
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
