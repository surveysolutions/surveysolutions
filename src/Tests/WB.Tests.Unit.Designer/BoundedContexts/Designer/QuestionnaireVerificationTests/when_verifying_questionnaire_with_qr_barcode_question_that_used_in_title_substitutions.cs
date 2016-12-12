using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_qr_barcode_question_that_used_in_title_substitutions : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocument(new TextQuestion()
            {
                PublicKey = questionWithSubstitutionToQRBarcodeId,
                StataExportCaption = "var",
                QuestionText = string.Format("question with substitution to %{0}%", qrBarcodeQuestionVariableName)
            },
            new QRBarcodeQuestion()
            {
                PublicKey = qrBarcodeQuestionId,
                StataExportCaption = qrBarcodeQuestionVariableName
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_messages_be_empty = () =>
            verificationMessages.ShouldBeEmpty();

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid qrBarcodeQuestionId = Guid.Parse("10000000000000000000000000000000");
        private static string qrBarcodeQuestionVariableName = "qrBarcodeQuestion";
        private static Guid questionWithSubstitutionToQRBarcodeId = Guid.Parse("11000000000000000000000000000000");
    }
}