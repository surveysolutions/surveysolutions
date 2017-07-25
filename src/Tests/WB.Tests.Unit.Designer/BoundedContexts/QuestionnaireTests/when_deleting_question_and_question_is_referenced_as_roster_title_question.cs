using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_deleting_question_and_question_is_referenced_as_roster_title_question : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");
            rosterTitleQuestionId = Guid.Parse("21111111111111111111111111111111");
            var rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterTitle = "Roster Title";

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddNumericQuestion(rosterSizeQuestionId,
                chapterId,responsibleId, isInteger:true);
            
            questionnaire.AddGroup(rosterId, title: rosterTitle, responsibleId: responsibleId, isRoster: true, rosterSourceType: RosterSizeSourceType.Question,
                rosterSizeQuestionId: rosterSizeQuestionId, rosterFixedTitles: null);

            questionnaire.AddTextQuestion(rosterTitleQuestionId,rosterId,responsibleId);

            questionnaire.UpdateGroup(rosterId, responsibleId, rosterTitle, "", rosterSizeQuestionId, "", null,false,true,
                RosterSizeSourceType.Question,null, rosterTitleQuestionId);
            BecauseOf();

        }

        private void BecauseOf() =>
            exception = Catch.Exception(() =>
                questionnaire.DeleteQuestion(rosterTitleQuestionId, responsibleId));

        [NUnit.Framework.Test] public void should_throw_QuestionnaireException () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__roster__ () =>
            exception.Message.ToLower().ShouldContain("roster");

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__using__ () =>
            exception.Message.ToLower().ShouldContain("title");

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__referenced__ () =>
            exception.Message.ToLower().ShouldContain("referenced");

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting_group_title () =>
            exception.Message.ShouldContain(rosterTitle);

        private static Exception exception;
        private static string rosterTitle;
        private static Questionnaire questionnaire;
        private static Guid rosterSizeQuestionId;
        private static Guid rosterTitleQuestionId;
        private static Guid responsibleId;
    }
}