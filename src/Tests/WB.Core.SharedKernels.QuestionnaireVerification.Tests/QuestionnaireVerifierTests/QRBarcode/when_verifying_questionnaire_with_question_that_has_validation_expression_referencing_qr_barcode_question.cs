using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Tests.QuestionnaireVerifierTests.QRBarcode
{
    internal class when_verifying_questionnaire_with_question_that_has_validation_expression_referencing_qr_barcode_question : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionWithValidationExpressionId = Guid.Parse("10000000000000000000000000000000");
            barcodeQuestionId = Guid.Parse("12222222222222222222222222222222");

            questionnaire = CreateQuestionnaireDocument();
            questionnaire.Children.Add(new QRBarcodeQuestion { PublicKey = barcodeQuestionId, StataExportCaption = "var1" });
            questionnaire.Children.Add(new NumericQuestion
            {
                PublicKey = questionWithValidationExpressionId,
                ValidationExpression = "some validation",
                ValidationMessage = "some message",
                StataExportCaption = "var2"
            });

            var expressionProcessor = Mock.Of<IExpressionProcessor>(processor
                => processor.IsSyntaxValid(it.IsAny<string>()) == true
                && processor.GetIdentifiersUsedInExpression(it.IsAny<string>()) == new[] { barcodeQuestionId.ToString() });

            verifier = CreateQuestionnaireVerifier(expressionProcessor);
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0052__ = () =>
            resultErrors.Single().Code.ShouldEqual("WB0052");

        It should_return_error_with_two_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(2);

        It should_return_first_error_reference_with_type_Question = () =>
            resultErrors.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_first_error_reference_with_id_of_question_with_validation_expression = () =>
            resultErrors.Single().References.First().Id.ShouldEqual(questionWithValidationExpressionId);

        It should_return_second_error_reference_with_type_Question = () =>
            resultErrors.Single().References.Second().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_second_error_reference_with_id_of_barcode_question = () =>
            resultErrors.Single().References.Second().Id.ShouldEqual(barcodeQuestionId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid questionWithValidationExpressionId;
        private static Guid barcodeQuestionId;
    }
}