using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_qr_barcode_question_used_as_linked_source_question : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocument();

            questionnaire.Children.Add(new NumericQuestion()
            {
                PublicKey = rosterSizeQuestionId,
                IsInteger = true,
                StataExportCaption = "var1"
            });
            questionnaire.Children.Add(new MultyOptionsQuestion()
            {
                PublicKey = multiQuestionLinkedToQRBarcodeQuestionId,
                StataExportCaption = "var2",
                LinkedToQuestionId = qrBarcodeQuestionId,
                Answers = { new Answer() { AnswerValue = "1", AnswerText = "opt 1" }, new Answer() { AnswerValue = "2", AnswerText = "opt 2" } }
            });
            questionnaire.Children.Add(new Group()
            {
                PublicKey = groupId,
                IsRoster = true,
                VariableName = "a",
                RosterSizeSource = RosterSizeSourceType.Question,
                RosterSizeQuestionId = rosterSizeQuestionId,
                Children = new List<IComposite>()
                {
                    new QRBarcodeQuestion()
                    {
                        PublicKey = qrBarcodeQuestionId,
                        StataExportCaption = "var3"
                    }
                }
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0012 = () =>
            resultErrors.Single().Code.ShouldEqual("WB0012");

        It should_return_error_with_2_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(2);

        private It should_return_error_reference_with_type_Question = () =>
            resultErrors.Single()
                .References.ShouldEachConformTo(reference => reference.Type == QuestionnaireVerificationReferenceType.Question);

        It should_return_error_reference_with_id_of_multiQuestionLinkedToQRBarcodeQuestionId = () =>
            resultErrors.Single().References.ElementAt(0).Id.ShouldEqual(multiQuestionLinkedToQRBarcodeQuestionId);

        It should_return_error_reference_with_id_of_qrBarcodeQuestionId = () =>
            resultErrors.Single().References.ElementAt(1).Id.ShouldEqual(qrBarcodeQuestionId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid multiQuestionLinkedToQRBarcodeQuestionId = Guid.Parse("a0000000000000000000000000000000");
        private static Guid qrBarcodeQuestionId = Guid.Parse("b0000000000000000000000000000000");
        private static Guid rosterSizeQuestionId = Guid.Parse("c0000000000000000000000000000000");
        private static Guid groupId = Guid.Parse("30000000000000000000000000000000");
    }
}