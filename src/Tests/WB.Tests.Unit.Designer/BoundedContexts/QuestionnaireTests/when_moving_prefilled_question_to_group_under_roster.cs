using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_moving_prefilled_question_to_group_under_roster : QuestionnaireTestsContext
    {
        private [NUnit.Framework.OneTimeSetUp] public void context () {
             questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
             questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
             questionnaire.AddGroup(rosterId,chapterId, responsibleId: responsibleId, isRoster: true);
             questionnaire.AddGroup( groupFromRosterId,  rosterId, responsibleId: responsibleId);
             questionnaire.AddTextQuestion(questionId, isPreFilled: true, parentId: chapterId, responsibleId:responsibleId);
    }

    private void BecauseOf() =>
        exception = Catch.Exception(() =>
            questionnaire.MoveQuestion(questionId, groupFromRosterId, 0, responsibleId));

    [NUnit.Framework.Test] public void should_throw_QuestionnaireException () =>
        exception.ShouldBeOfExactType<QuestionnaireException>();

    [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__prefilled__ () =>
        exception.Message.ToLower().ShouldContain("identifying");

    [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__roster__ () =>
        exception.Message.ToLower().ShouldContain("roster");

    private static Exception exception;
    private static Questionnaire questionnaire;
    private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
    private static Guid rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    private static Guid groupFromRosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
    private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
}
}