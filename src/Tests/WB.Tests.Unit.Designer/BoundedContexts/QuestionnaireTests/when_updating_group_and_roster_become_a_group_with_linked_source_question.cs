using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_updating_group_and_roster_become_a_group_with_linked_source_question : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.AddQuestion(Create.Event.NewQuestionAdded(
                publicKey : categoricalLinkedQuestionId,
                groupPublicKey : chapterId,
                questionType : QuestionType.MultyOption,
                linkedToQuestionId : linkedSourceQuestionId
            ));
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = rosterId, ParentGroupPublicKey = chapterId });
            questionnaire.MarkGroupAsRoster(new GroupBecameARoster(responsibleId, rosterId));
            questionnaire.AddQuestion(Create.Event.NewQuestionAdded(
                publicKey : linkedSourceQuestionId,
                groupPublicKey : rosterId,
                questionType : QuestionType.Text
            ));
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.UpdateGroup(groupId: rosterId, responsibleId: responsibleId, title: "title", variableName: null, rosterSizeQuestionId: null,
                    description: null, condition: null, hideIfDisabled: false, isRoster: false, rosterSizeSource: RosterSizeSourceType.Question,
                    rosterFixedTitles: null, rosterTitleQuestionId: null));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__contains__ = () =>
            exception.Message.ToLower().ShouldContain("contains");

        It should_throw_exception_with_message_containting__linked__ = () =>
            exception.Message.ToLower().ShouldContain("linked");

        It should_throw_exception_with_message_containting__source__ = () =>
            exception.Message.ToLower().ShouldContain("source");

        It should_throw_exception_with_message_containting__question__ = () =>
            exception.Message.ToLower().ShouldContain("question");

        private static Exception exception;
        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid rosterId = Guid.Parse("11111111111111111111111111111111");
        private static Guid categoricalLinkedQuestionId = Guid.Parse("33333333333333333333333333333333");
        private static Guid linkedSourceQuestionId = Guid.Parse("44444444444444444444444444444444");
    }
}