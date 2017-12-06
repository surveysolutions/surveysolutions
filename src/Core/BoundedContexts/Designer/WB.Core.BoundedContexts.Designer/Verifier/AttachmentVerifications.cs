using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Designer.Verifier
{
    public class AttachmentVerifications : AbstractVerifier, IPartialVerifier
    {
        private readonly IAttachmentService attachmentService;

        public AttachmentVerifications(IAttachmentService attachmentService)
        {
            this.attachmentService = attachmentService;
        }
        private IEnumerable<Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>>> ErrorsVerifiers => new[]
        {
            ErrorForAttachment(AttachmentHasEmptyContent, "WB0111", VerificationMessages.WB0111_AttachmentHasEmptyContent),
            ErrorsByAttachmentsWithDuplicateName,
            Warning(AttachmentSizeIsMoreThan5Mb, "WB0213", string.Format(VerificationMessages.WB0213_AttachmentSizeIsMoreThan5Mb, MaxAttachmentSizeInMb)),
            Warning(TotalAttachmentsSizeIsMoreThan50Mb, "WB0214", string.Format(VerificationMessages.WB0214_TotalAttachmentsSizeIsMoreThan50Mb, MaxAttachmentsSizeInMb)),
            Warning(UnusedAttachments, "WB0215", VerificationMessages.WB0215_UnusedAttachments),
        };

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Warning(
            Func<MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
        {
            return questionnaire =>
                hasError(questionnaire)
                    ? new[] { QuestionnaireVerificationMessage.Warning(code, message) }
                    : Enumerable.Empty<QuestionnaireVerificationMessage>();
        }

        private static bool AttachmentSizeIsMoreThan5Mb(AttachmentSize attachmentSize, MultiLanguageQuestionnaireDocument questionnaire)
            => questionnaire.Attachments.Any(x => x.AttachmentId == attachmentSize.AttachmentId) && attachmentSize.Size > 5 * 1024 * 1024;

        private static bool UnusedAttachments(Attachment attachment, MultiLanguageQuestionnaireDocument questionnaire)
            => !questionnaire
                .Find<IStaticText>(t => t.AttachmentName == attachment.Name)
                .Any();

        private bool TotalAttachmentsSizeIsMoreThan50Mb(MultiLanguageQuestionnaireDocument questionnaire)
            => this.attachmentService
                   .GetAttachmentSizesByQuestionnaire(questionnaire.PublicKey)
                   .Sum(x => x.Size) > 50 * 1024 * 1024;

        private static IEnumerable<QuestionnaireVerificationMessage> ErrorsByAttachmentsWithDuplicateName(
            MultiLanguageQuestionnaireDocument questionnaire)
        {
            return questionnaire
                .Attachments
                .Where(x => !string.IsNullOrEmpty(x.Name))
                .GroupBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group =>
                    QuestionnaireVerificationMessage.Error(
                        "WB0065",
                        VerificationMessages.WB0065_NameForAttachmentIsNotUnique,
                        group.Select(e => QuestionnaireNodeReference.CreateForAttachment(e.AttachmentId)).ToArray()));
        }

        private bool AttachmentHasEmptyContent(Attachment attachment, MultiLanguageQuestionnaireDocument questionnaire)
        {
            var attachmentContent = this.attachmentService.GetContentDetails(attachment.ContentId);
            return attachmentContent == null || attachmentContent.Size == 0;
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> ErrorForAttachment(
            Func<Attachment, MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
        {
            return (questionnaire) => questionnaire
                .Attachments
                .Where(entity => hasError(entity, questionnaire))
                .Select(entity => QuestionnaireVerificationMessage.Error(code, message, QuestionnaireNodeReference.CreateForAttachment(entity.AttachmentId)));
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Warning(
            Func<Attachment, MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
        {
            return questionnaire =>
                questionnaire
                    .Attachments
                    .Where(x => hasError(x, questionnaire))
                    .Select(entity => QuestionnaireVerificationMessage.Warning(code, message, QuestionnaireNodeReference.CreateForAttachment(entity.AttachmentId)));
        }

        private Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Warning(
            Func<AttachmentSize, MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
        {
            return questionnaire =>
                this.attachmentService.GetAttachmentSizesByQuestionnaire(questionnaire.PublicKey)
                    .Where(x => hasError(x, questionnaire))
                    .Select(entity => QuestionnaireVerificationMessage.Warning(code, message, QuestionnaireNodeReference.CreateForAttachment(entity.AttachmentId)));
        }

        public IEnumerable<QuestionnaireVerificationMessage> Verify(MultiLanguageQuestionnaireDocument multiLanguageQuestionnaireDocument)
        {
            var verificationMessagesByQuestionnaire = new List<QuestionnaireVerificationMessage>();
            foreach (var verifier in ErrorsVerifiers)
            {
                verificationMessagesByQuestionnaire.AddRange(verifier.Invoke(multiLanguageQuestionnaireDocument));
            }
            return verificationMessagesByQuestionnaire;
        }
    }
}