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
        private Establish context = () =>
        {
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
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_messages_be_empty = () => verificationMessages.ShouldBeEmpty();

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid qrBarcodeQuestionId = Guid.Parse("10000000000000000000000000000000");
        private static string qrBarcodeQuestionVariableName = "qrBarcodeQuestion";
        private static Guid questionWithSubstitutionToQRBarcodeId = Guid.Parse("11000000000000000000000000000000");
    }
}