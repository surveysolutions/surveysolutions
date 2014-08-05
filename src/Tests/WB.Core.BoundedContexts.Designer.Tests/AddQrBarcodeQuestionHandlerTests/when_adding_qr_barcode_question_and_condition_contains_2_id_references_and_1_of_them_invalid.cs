using System;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.AddQrBarcodeQuestionHandlerTests
{
    internal class when_adding_qr_barcode_question_and_condition_contains_2_id_references_and_1_of_them_invalid : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NumericQuestionAdded() { PublicKey = existingQuestionId, GroupPublicKey = chapterId });

            RegisterExpressionProcessorMock(conditionExpression, new[] { existingQuestionId.ToString(), notExistingQuestionId.ToString() });

        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.AddQRBarcodeQuestion(questionId: questionId, parentGroupId: chapterId, title: "title",
                    variableName: "var",
                variableLabel: null, isMandatory: false, enablementCondition: conditionExpression, instructions: null,
                    responsibleId: responsibleId));

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
        private static string conditionExpression = string.Format("[{0}] > 1 and [{1}] < 2", existingQuestionId, notExistingQuestionId);
    }
}