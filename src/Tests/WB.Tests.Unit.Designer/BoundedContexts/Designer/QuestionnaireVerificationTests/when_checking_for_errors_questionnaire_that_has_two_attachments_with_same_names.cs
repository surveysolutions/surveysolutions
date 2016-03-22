using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    class when_checking_for_errors_questionnaire_that_has_two_attachments_with_same_names : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            var attachments = new[] { Create.Attachment(attachment1Id, "hello"), Create.Attachment(attachment2Id, "hello") };
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(questionId, 
                attachments: attachments,
                children: Create.TextQuestion(variable: "var"));

            var questionnaireAttachment = Mock.Of<QuestionnaireAttachment>(a => a.Content == content);
            attachmentServiceMock.Setup(x => x.GetAttachment(attachment1Id)).Returns(questionnaireAttachment);
            attachmentServiceMock.Setup(x => x.GetAttachment(attachment2Id)).Returns(questionnaireAttachment);
            attachmentServiceMock.Setup(x => x.GetAttachmentSizesByQuestionnaire(Moq.It.IsAny<Guid>()))
                .Returns(attachments.Select(y => new AttachmentSize { Size = 10 }).ToList());

            attachmentServiceMock.Setup(x => x.GetContentDetails(Moq.It.IsAny<string>()))
                .Returns(new AttachmentContentView { Size = 10 });

            verifier = CreateQuestionnaireVerifier(attachmentService: attachmentServiceMock.Object);
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(questionnaire);

        It should_return_1_message = () =>
            verificationMessages.Count().ShouldEqual(1);

        It should_return_message_with_code__WB0065 = () =>
            verificationMessages.Single().Code.ShouldEqual("WB0065");

        It should_return_message_with_General_level = () =>
            verificationMessages.Single().MessageLevel.ShouldEqual(VerificationMessageLevel.General);

        It should_return_message_with_1_reference = () =>
            verificationMessages.Single().References.Count().ShouldEqual(2);

        It should_return_message_reference_with_type_Attachment = () =>
            verificationMessages.Single().References.ShouldEachConformTo(reference => reference.Type == QuestionnaireVerificationReferenceType.Attachment);

        It should_return_message_reference_with_id_of_attachment1Id = () =>
            verificationMessages.Single().References.ElementAt(0).Id.ShouldEqual(attachment1Id);

        It should_return_message_reference_with_id_of_attachment2Id = () =>
            verificationMessages.Single().References.ElementAt(1).Id.ShouldEqual(attachment2Id);

        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;

        private static Mock<IAttachmentService> attachmentServiceMock = new Mock<IAttachmentService>();

        private static readonly Guid attachment1Id = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid attachment2Id = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid questionId = Guid.Parse("10000000000000000000000000000000");
        private static readonly byte[] content = new byte[] { 1, 2, 3};
    }
}