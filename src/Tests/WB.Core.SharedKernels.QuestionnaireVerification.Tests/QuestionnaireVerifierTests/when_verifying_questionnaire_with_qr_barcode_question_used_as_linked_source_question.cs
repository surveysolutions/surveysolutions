using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.QuestionnaireVerification.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Tests.QuestionnaireVerifierTests
{
    internal class when_verifying_questionnaire_with_qr_barcode_question_used_as_linked_source_question : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocument();

            questionnaire.Children.Add(new NumericQuestion() { PublicKey = rosterSizeQuestionId, IsInteger = true, MaxValue = 5 });
            questionnaire.Children.Add(new MultyOptionsQuestion()
            {
                PublicKey = multiQuestionLinkedToQRBarcodeQuestionId,
                LinkedToQuestionId = qrBarcodeQuestionId
            });
            questionnaire.Children.Add(new Group()
            {
                PublicKey = groupId,
                IsRoster = true,
                RosterSizeSource = RosterSizeSourceType.Question,
                RosterSizeQuestionId = rosterSizeQuestionId,
                Children = new List<IComposite>()
                {
                    new QRBarcodeQuestion()
                    {
                        PublicKey = qrBarcodeQuestionId
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
            resultErrors.Single().References.ElementAt(1).Id.ShouldEqual(multiQuestionLinkedToQRBarcodeQuestionId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid multiQuestionLinkedToQRBarcodeQuestionId = Guid.Parse("10000000000000000000000000000000");
        private static Guid qrBarcodeQuestionId = Guid.Parse("10000000000000000000000000000000");
        private static Guid rosterSizeQuestionId = Guid.Parse("20000000000000000000000000000000");
        private static Guid groupId = Guid.Parse("30000000000000000000000000000000");
    }
}