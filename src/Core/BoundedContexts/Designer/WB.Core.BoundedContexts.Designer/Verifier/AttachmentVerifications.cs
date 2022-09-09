using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Designer.Verifier
{
    public class AttachmentVerifications : AbstractVerifier, IPartialVerifier
    {
        private readonly IAttachmentService attachmentService;
        private readonly IKeywordsProvider keywordsProvider;

        public AttachmentVerifications(IAttachmentService attachmentService, IKeywordsProvider keywordsProvider)
        {
            this.attachmentService = attachmentService;
            this.keywordsProvider = keywordsProvider;
        }

        private IEnumerable<Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>>>
            ErrorsVerifiers => new[]
        {
            ErrorForAttachment(AttachmentHasEmptyContent, "WB0111",
                VerificationMessages.WB0111_AttachmentHasEmptyContent),
            ErrorForAttachment(AttachmentHasInvalidName, "WB0315",
                VerificationMessages.WB0315_AttachmentHasInvalidName),
            ErrorsByAttachmentsWithDuplicateName,
            Warning(AttachmentSizeIsMoreThan5Mb, "WB0213",
                string.Format(VerificationMessages.WB0213_AttachmentSizeIsMoreThan5Mb, MaxAttachmentSizeInMb)),
            Warning(TotalAttachmentsSizeIsMoreThan50Mb, "WB0214",
                string.Format(VerificationMessages.WB0214_TotalAttachmentsSizeIsMoreThan50Mb, MaxAttachmentsSizeInMb)),
            Warning(UnusedAttachments, "WB0215", VerificationMessages.WB0215_UnusedAttachments),
        };

        private bool AttachmentHasInvalidName(Attachment attachment, MultiLanguageQuestionnaireDocument questionnaire)
        {
            var name = attachment.Name;

            if (string.IsNullOrWhiteSpace(name))
                return true;

            if (name.Length > DefaultVariableLengthLimit
                || name[^1] == '_'
                || name.Contains("__")
                || keywordsProvider.IsReservedKeyword(name)
                || Char.IsDigit(name[0]) || name[0] == '_')
                return true;

            foreach (var c in name)
            {
                if (c != '_' && !Char.IsDigit(c) && !((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')))
                    return true;
            }

            return false;
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Warning(
            Func<MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
        {
            return questionnaire =>
                hasError(questionnaire)
                    ? new[] { QuestionnaireVerificationMessage.Warning(code, message) }
                    : Enumerable.Empty<QuestionnaireVerificationMessage>();
        }

        private static bool AttachmentSizeIsMoreThan5Mb(AttachmentSize attachmentSize,
            MultiLanguageQuestionnaireDocument questionnaire)
            => questionnaire.Attachments.Any(x => x.AttachmentId == attachmentSize.AttachmentId) &&
               attachmentSize.Size > 5 * 1024 * 1024;

        private static bool UnusedAttachments(Attachment attachment, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (questionnaire.Find<IStaticText>(t => t.AttachmentName == attachment.Name).Any())
                return false;

            if (questionnaire.Categories.Any(t => t.AttachmentName == attachment.Name))
                return false;

            if (questionnaire.Categories.Any(t => t.AttachmentName == attachment.Name))
                return false;

            if (questionnaire.Find<ICategoricalQuestion>(t => t.Answers.Any(x => x.AttachmentName == attachment.Name)).Any())
                return false;

            return true;
        }

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
                        group.Select(e => QuestionnaireEntityReference.CreateForAttachment(e.AttachmentId)).ToArray()));
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
                .Select(entity => QuestionnaireVerificationMessage.Error(code, message, QuestionnaireEntityReference.CreateForAttachment(entity.AttachmentId)));
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Warning(
            Func<Attachment, MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
        {
            return questionnaire =>
                questionnaire
                    .Attachments
                    .Where(x => hasError(x, questionnaire))
                    .Select(entity => QuestionnaireVerificationMessage.Warning(code, message, QuestionnaireEntityReference.CreateForAttachment(entity.AttachmentId)));
        }

        private Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Warning(
            Func<AttachmentSize, MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
        {
            return questionnaire =>
                this.attachmentService.GetAttachmentSizesByQuestionnaire(questionnaire.PublicKey)
                    .Where(x => hasError(x, questionnaire))
                    .Select(entity => QuestionnaireVerificationMessage.Warning(code, message, QuestionnaireEntityReference.CreateForAttachment(entity.AttachmentId)));
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
