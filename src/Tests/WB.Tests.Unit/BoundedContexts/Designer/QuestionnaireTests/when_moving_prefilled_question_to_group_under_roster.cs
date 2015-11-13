using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using System;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_moving_prefilled_question_to_group_under_roster : QuestionnaireTestsContext
    {
        private Establish context = () =>
         {
             questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
             questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
             questionnaire.Apply(new NewGroupAdded { PublicKey = rosterId, ParentGroupPublicKey = chapterId });
             questionnaire.Apply(new GroupBecameARoster(responsibleId, rosterId));
             questionnaire.Apply(new NewGroupAdded { PublicKey = groupFromRosterId, ParentGroupPublicKey = rosterId });
             questionnaire.Apply(Create.Event.NewQuestionAdded(publicKey: questionId, featured: true, groupPublicKey : chapterId, questionType : QuestionType.Text ));
    };

    Because of = () =>
        exception = Catch.Exception(() =>
            questionnaire.MoveQuestion(questionId, groupFromRosterId, 0, responsibleId));

    It should_throw_QuestionnaireException = () =>
        exception.ShouldBeOfExactType<QuestionnaireException>();

    It should_throw_exception_with_message_containting__prefilled__ = () =>
        exception.Message.ToLower().ShouldContain("pre-filled");

    It should_throw_exception_with_message_containting__roster__ = () =>
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