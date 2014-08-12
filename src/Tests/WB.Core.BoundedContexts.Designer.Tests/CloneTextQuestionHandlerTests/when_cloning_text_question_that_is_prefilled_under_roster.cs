using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests;

namespace WB.Core.BoundedContexts.Designer.Tests.CloneTextQuestionHandlerTests
{
    internal class when_cloning_text_question_that_is_prefilled_under_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = parentGroupId });
            questionnaire.Apply(new NewQuestionAdded { PublicKey = sourceQuestionId, QuestionType = QuestionType.Text, GroupPublicKey = parentGroupId });
            questionnaire.Apply(new NewGroupAdded { PublicKey = rosterId });
            questionnaire.Apply(new GroupBecameARoster(responsibleId, rosterId));
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                  questionnaire.CloneTextQuestion(
                    questionId: questionId,
                    title: title,
                    variableName: variableName,
                variableLabel: null,
                    isMandatory: isMandatory,
                    isPreFilled: isPrefilled,
                    scope: scope,
                    enablementCondition: enablementCondition,
                    validationExpression: validationExpression,
                    validationMessage: validationMessage,
                    instructions: instructions,
                     mask: null,
                    parentGroupId: rosterId,
                    sourceQuestionId: sourceQuestionId,
                    targetIndex: targetIndex,
                    responsibleId: responsibleId));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__categorical_could_not_be_roster_title_question__ = () =>
            new[] { "pre-filled", "roster" }.ShouldEachConformTo(
                keyword => exception.Message.ToLower().Contains(keyword));

        private static Exception exception;
        private static Questionnaire questionnaire;

        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid parentGroupId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid sourceQuestionId = Guid.Parse("44444444444444444444444444444444");
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static string variableName = "var";
        private static bool isPrefilled = true;
        private static bool isMandatory = true;
        private static string title = "title";
        private static string instructions = "intructions";
        private static int targetIndex = 1;
        private static QuestionScope scope = QuestionScope.Interviewer;
        private static string enablementCondition = null;
        private static string validationExpression = null;
        private static string validationMessage = null;
    }
}