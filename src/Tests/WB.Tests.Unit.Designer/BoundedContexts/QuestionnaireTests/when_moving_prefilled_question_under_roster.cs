using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_moving_prefilled_question_under_roster : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionId = Guid.Parse("11111111111111111111111111111111");

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddGroup(rosterId,chapterId, responsibleId: responsibleId, isRoster: true);
            
            questionnaire.AddTextQuestion(questionId, chapterId, responsibleId, isPreFilled : true );
            BecauseOf();
        }

        private void BecauseOf() =>
            exception = Catch.Exception(() =>
                questionnaire.MoveQuestion(questionId, rosterId, 0, responsibleId));

        [NUnit.Framework.Test] public void should_throw_QuestionnaireException () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__prefilled__ () =>
            exception.Message.ToLower().ShouldContain("identifying");

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__roster__ () =>
            exception.Message.ToLower().ShouldContain("roster");

        private static Exception exception;
        private static Questionnaire questionnaire;
        private static Guid questionId;
        private static Guid rosterId;
        private static Guid responsibleId;
    }
}