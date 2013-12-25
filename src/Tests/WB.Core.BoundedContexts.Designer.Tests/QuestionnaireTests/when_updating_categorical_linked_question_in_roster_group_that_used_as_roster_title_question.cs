using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    internal class when_updating_categorical_linked_question_in_roster_group_that_used_as_roster_title_question :
        QuestionnaireTestsContext
    {
        private Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var anotherRosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");
            rosterTitleQuestionId = Guid.Parse("22222222222222222222222222222222");
            linkedToQuestionId = Guid.Parse("33333333333333333333333333333333");

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NewGroupAdded { PublicKey = anotherRosterId });
            questionnaire.Apply(new GroupBecameARoster(responsibleId, anotherRosterId));
            questionnaire.Apply(new NumericQuestionAdded
            {
                PublicKey = rosterSizeQuestionId,
                IsInteger = true,
                GroupPublicKey = anotherRosterId
            });
            questionnaire.Apply(new NewQuestionAdded
            {
                PublicKey = rosterTitleQuestionId,
                GroupPublicKey = anotherRosterId,
                QuestionType = QuestionType.MultyOption
            });
            questionnaire.Apply(new NewGroupAdded { PublicKey = groupId });
            questionnaire.Apply(new GroupBecameARoster(responsibleId, groupId));
            questionnaire.Apply(new RosterChanged(responsibleId, groupId, rosterSizeQuestionId, RosterSizeSourceType.Question, null,
                rosterTitleQuestionId));
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.NewUpdateQuestion(questionId: rosterTitleQuestionId, title: "title", type: QuestionType.MultyOption,
                    alias: "q1", isMandatory: false, isFeatured: false, scope: QuestionScope.Interviewer, condition: null,
                    validationExpression: null, validationMessage: null, instructions: null, options: null, optionsOrder: Order.AsIs,
                    responsibleId: responsibleId, linkedToQuestionId: linkedToQuestionId, areAnswersOrdered: false, maxAllowedAnswers: null));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__categorical__ = () =>
            exception.Message.ToLower().ShouldContain("categorical");

        It should_throw_exception_with_message_containting__linked__ = () =>
            exception.Message.ToLower().ShouldContain("linked");

        It should_throw_exception_with_message_containting__question__ = () =>
            exception.Message.ToLower().ShouldContain("question");

        It should_throw_exception_with_message_containting__not__ = () =>
            exception.Message.ToLower().ShouldContain("not");

        It should_throw_exception_with_message_containting__be__ = () =>
            exception.Message.ToLower().ShouldContain("be");

        It should_throw_exception_with_message_containting__roster__ = () =>
            exception.Message.ToLower().ShouldContain("roster");

        It should_throw_exception_with_message_containting__title__ = () =>
            exception.Message.ToLower().ShouldContain("title");

        private static Exception exception;
        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid groupId;
        private static Guid rosterSizeQuestionId;
        private static Guid rosterTitleQuestionId;
        private static Guid linkedToQuestionId;
    }
}