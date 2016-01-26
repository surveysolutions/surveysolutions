using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_qr_barcode_question_that_used_in_title_substitutions : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocument();

            questionnaire.Children.Add(new TextQuestion()
            {
                PublicKey = questionWithSubstitutionToQRBarcodeId,
                StataExportCaption = "var",
                QuestionText = string.Format("question with substitution to %{0}%", qrBarcodeQuestionVariableName)
            });
            questionnaire.Children.Add(new QRBarcodeQuestion()
            {
                PublicKey = qrBarcodeQuestionId,
                StataExportCaption = qrBarcodeQuestionVariableName
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0018 = () =>
            resultErrors.Single().Code.ShouldEqual("WB0018");

        It should_return_error_with_1_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(2);

        It should_return_error_reference_with_type_Question = () =>
            resultErrors.Single().References.ShouldEachConformTo(reference => reference.Type == QuestionnaireVerificationReferenceType.Question);

        It should_return_error_reference_with_id_of_questionWithSubstitutionToQRBarcodeId = () =>
            resultErrors.Single().References.ElementAt(0).Id.ShouldEqual(questionWithSubstitutionToQRBarcodeId);

        It should_return_error_reference_with_id_of_qrBarcodeQuestionId = () =>
            resultErrors.Single().References.ElementAt(1).Id.ShouldEqual(qrBarcodeQuestionId);

        private static IEnumerable<QuestionnaireVerificationMessage> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid qrBarcodeQuestionId = Guid.Parse("10000000000000000000000000000000");
        private static string qrBarcodeQuestionVariableName = "qrBarcodeQuestion";
        private static Guid questionWithSubstitutionToQRBarcodeId = Guid.Parse("11000000000000000000000000000000");
    }
}