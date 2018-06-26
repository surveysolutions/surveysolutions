using System;
using System.Collections.Generic;
using FluentAssertions;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_qr_barcode_question_that_used_in_title_substitutions : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaireDocument(
                Create.TextQuestion(
                    questionWithSubstitutionToQRBarcodeId,
                    variable: "var",
                    text: $"question with substitution to %{qrBarcodeQuestionVariableName}%"
                ),
                Create.QRBarcodeQuestion(
                    qrBarcodeQuestionId,
                    variable: qrBarcodeQuestionVariableName
                ));

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_messages_be_empty () => verificationMessages.Should().BeEmpty();

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid qrBarcodeQuestionId = Guid.Parse("10000000000000000000000000000000");
        private static string qrBarcodeQuestionVariableName = "qrBarcodeQuestion";
        private static Guid questionWithSubstitutionToQRBarcodeId = Guid.Parse("11000000000000000000000000000000");
    }
}
