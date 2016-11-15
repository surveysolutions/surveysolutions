using System;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Attachments;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.Attachments
{
    internal class when_updating_attachment : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: responsibleId);
            questionnaire.AddOrUpdateAttachment(Create.Command.AddOrUpdateAttachment(questionnaireId, oldAttachmentId, "", responsibleId, ""));

            updateAttachment = Create.Command.AddOrUpdateAttachment(questionnaireId, attachmentId, attachmentContentId, responsibleId, name, oldAttachmentId);
        };


        Because of = () => questionnaire.AddOrUpdateAttachment(updateAttachment);

        It should_contains_attachment_with_EntityId_specified = () =>
            questionnaire.QuestionnaireDocument.Attachments.Single(a => a.AttachmentId == attachmentId).AttachmentId.ShouldEqual(attachmentId);

        It should_contains_attachment_with_AttachmentName_specified = () =>
            questionnaire.QuestionnaireDocument.Attachments.Single(a => a.AttachmentId == attachmentId).Name.ShouldEqual(name);

        It should_contains_attachment_with_ContentId_specified = () =>
            questionnaire.QuestionnaireDocument.Attachments.Single(a => a.AttachmentId == attachmentId).ContentId.ShouldEqual(attachmentContentId);

        private static AddOrUpdateAttachment updateAttachment;
        private static Questionnaire questionnaire;
        private static readonly string name = "Attachment";
        private static readonly string attachmentContentId = "ABECA98D65F866DFCD292BC973BDACF5954B916D";
        private static readonly Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid attachmentId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid oldAttachmentId = Guid.Parse("1AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
}