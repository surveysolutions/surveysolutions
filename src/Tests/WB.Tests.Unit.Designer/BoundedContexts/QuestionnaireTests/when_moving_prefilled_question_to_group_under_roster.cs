using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_moving_prefilled_question_to_group_under_roster : QuestionnaireTestsContext
    {
        private Establish context = () =>
         {
             questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
             questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });
             questionnaire.AddGroup(new NewGroupAdded { PublicKey = rosterId, ParentGroupPublicKey = chapterId });
             questionnaire.MarkGroupAsRoster(new GroupBecameARoster(responsibleId, rosterId));
             questionnaire.AddGroup(new NewGroupAdded { PublicKey = groupFromRosterId, ParentGroupPublicKey = rosterId });
             questionnaire.AddTextQuestion(questionId, isPreFilled: true, parentId: chapterId, responsibleId:responsibleId);
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