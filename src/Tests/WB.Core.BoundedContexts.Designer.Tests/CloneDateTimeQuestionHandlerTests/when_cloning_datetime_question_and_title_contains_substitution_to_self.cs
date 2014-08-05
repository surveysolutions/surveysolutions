using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests;

namespace WB.Core.BoundedContexts.Designer.Tests.CloneDateTimeQuestionHandlerTests
{
    internal class when_cloning_datetime_question_and_title_contains_substitution_to_self : QuestionnaireTestsContext
    {
        private Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new QRBarcodeQuestionAdded()
            {
                QuestionId = sourceQuestionId,
                ParentGroupId = chapterId,
                Title = "old title",
                VariableName = "old_variable_name",
                IsMandatory = false,
                Instructions = "old instructions",
                EnablementCondition = "old condition",
                ResponsibleId = responsibleId
            });
        };

        private Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.CloneDateTimeQuestion(questionId: questionId,
                    title: titleWithSubstitutionToSelf,
                    variableName: variableName,
                variableLabel: null,
                    isMandatory: isMandatory,
                    isPreFilled: isPreFilled,
                    scope: scope,
                    enablementCondition: enablementCondition,
                    validationExpression: validationExpression,
                    validationMessage: validationMessage,
                    instructions: instructions,
                    parentGroupId: parentGroupId,
                    sourceQuestionId: sourceQuestionId,
                    targetIndex: targetIndex,
                    responsibleId: responsibleId));

        private It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        private It should_throw_exception_with_message_containting__title__contains__self__ = () =>
            new[] { "title", "contains", "self" }.ShouldEachConformTo(
                keyword => exception.Message.ToLower().Contains(keyword));


        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid sourceQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string variableName = "datetime_question";
        private static string titleWithSubstitutionToSelf = string.Format("title with substitution to self - %{0}%", variableName);
        private static Guid parentGroupId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static bool isMandatory = true;
        private static string instructions = "intructions";
        private static int targetIndex = 1;
        private static bool isPreFilled = false;
        private static QuestionScope scope = QuestionScope.Interviewer;
        private static string enablementCondition = null;
        private static string validationExpression = null;
        private static string validationMessage = null;
    }
}