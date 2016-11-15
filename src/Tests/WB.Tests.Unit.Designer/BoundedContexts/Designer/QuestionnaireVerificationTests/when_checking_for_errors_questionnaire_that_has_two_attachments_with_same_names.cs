using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    class when_checking_for_errors_questionnaire_that_has_two_attachments_with_same_names : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                attachments: new[] { Create.Attachment(attachment1Id, "hello"), Create.Attachment(attachment2Id, "hello") });

            var attachmentService = Setup.AttachmentsServiceForOneQuestionnaire(questionnaire.PublicKey);

            verifier = CreateQuestionnaireVerifier(attachmentService: attachmentService);
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));
       
        It should_return_message_with_code__WB0065 = () =>
            verificationMessages.ShouldContainError("WB0065");

        It should_return_message_with_1_reference = () =>
            verificationMessages.GetError("WB0065").References.Count().ShouldEqual(2);

        It should_return_message_reference_with_type_Attachment = () =>
            verificationMessages.GetError("WB0065").References.ShouldEachConformTo(reference => reference.Type == QuestionnaireVerificationReferenceType.Attachment);

        It should_return_message_reference_with_id_of_attachment1Id = () =>
            verificationMessages.GetError("WB0065").References.ElementAt(0).Id.ShouldEqual(attachment1Id);

        It should_return_message_reference_with_id_of_attachment2Id = () =>
            verificationMessages.GetError("WB0065").References.ElementAt(1).Id.ShouldEqual(attachment2Id);

        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;

        private static readonly Guid attachment1Id = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid attachment2Id = Guid.Parse("22222222222222222222222222222222");
    }
}