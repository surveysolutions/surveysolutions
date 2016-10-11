using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateTextQuestionHandlerTests
{
    [Ignore("reference validation is turned off")]
    internal class when_updating_text_question_and_validation_contains_2_id_references_and_1_of_them_invalid : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            var expressionProcessor = Mock.Of<IExpressionProcessor>(processor
                => processor.GetIdentifiersUsedInExpression(validationExpression) == new[] { existingQuestionId.ToString(), notExistingQuestionId.ToString() });

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddNumericQuestion(existingQuestionId, chapterId, responsibleId);
            questionnaire.AddQRBarcodeQuestion(questionId, chapterId,responsibleId,
                title: "old title", variableName: "old_variable_name",instructions: "old instructions",enablementCondition: "old condition");
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.UpdateTextQuestion(
                    new UpdateTextQuestion(
                        questionnaire.Id,
                        questionId,
                        responsibleId,
                        new CommonQuestionParameters() {Title = title,VariableName = variableName,EnablementCondition = enablementCondition,Instructions = instructions}, 
                        null,
                        scope,
                        isPreFilled,
                        new System.Collections.Generic.List<WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition>())));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__not__valid__expression__ = () =>
            new[] { "not", "valid", "expression" }.ShouldEachConformTo(
                keyword => exception.Message.ToLower().Contains(keyword));


        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid existingQuestionId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid notExistingQuestionId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string variableName = "qr_barcode_question";
        private static string title = "title";
        private static string instructions = "intructions";
        private static bool isPreFilled = false;
        private static QuestionScope scope = QuestionScope.Interviewer;
        private static string enablementCondition = null;
        private static string validationExpression = string.Format("[{0}] > 1 and [{1}] < 2", existingQuestionId, notExistingQuestionId);
    }
}