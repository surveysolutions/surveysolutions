#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.PdfInterview;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public interface IInterviewCompletedEmailBuilder
    {
        Task<PreparedInterviewCompletedEmail?> PrepareAsync(Guid interviewId, ISenderInformation senderInfo,
            CancellationToken cancellationToken);
    }

    // Only materialized message data leaves the preparation scope, never NHibernate entities or services.
    public class PreparedInterviewCompletedEmail
    {
        public string Address { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string Html { get; set; } = null!;
        public string Text { get; set; } = null!;
        public List<EmailAttachment> Attachments { get; set; } = new();
    }

    public class InterviewCompletedEmailBuilder : IInterviewCompletedEmailBuilder
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IWebInterviewConfigProvider webInterviewConfigProvider;
        private readonly IWebInterviewEmailRenderer webInterviewEmailRenderer;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IPdfInterviewGenerator pdfInterviewGenerator;
        private readonly IPlainKeyValueStorage<EmailParameters> emailParamsStorage;
        private readonly IAssignmentsService assignmentsService;

        public InterviewCompletedEmailBuilder(IStatefulInterviewRepository interviewRepository,
            IWebInterviewConfigProvider webInterviewConfigProvider, IWebInterviewEmailRenderer webInterviewEmailRenderer,
            IQuestionnaireStorage questionnaireStorage, IPdfInterviewGenerator pdfInterviewGenerator,
            IPlainKeyValueStorage<EmailParameters> emailParamsStorage, IAssignmentsService assignmentsService)
        {
            this.interviewRepository = interviewRepository;
            this.webInterviewConfigProvider = webInterviewConfigProvider;
            this.webInterviewEmailRenderer = webInterviewEmailRenderer;
            this.questionnaireStorage = questionnaireStorage;
            this.pdfInterviewGenerator = pdfInterviewGenerator;
            this.emailParamsStorage = emailParamsStorage;
            this.assignmentsService = assignmentsService;
        }

        public async Task<PreparedInterviewCompletedEmail?> PrepareAsync(Guid interviewId,
            ISenderInformation senderInfo, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var interview = interviewRepository.Get(interviewId.FormatGuid());
            if (interview == null)
                return null;

            var config = webInterviewConfigProvider.Get(interview.QuestionnaireIdentity);
            if (!config.EmailOnComplete)
                return null;

            var assignmentId = interview.GetAssignmentId();
            if (assignmentId == null)
                return null;

            var questionnaire = questionnaireStorage.GetQuestionnaire(interview.QuestionnaireIdentity, null);
            if (questionnaire == null)
                return null;

            var assignment = assignmentsService.GetAssignment(assignmentId.Value);
            if (assignment == null || string.IsNullOrEmpty(assignment.Email))
                return null;

            var attachments = new List<EmailAttachment>();
            if (config.AttachAnswersInEmail)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using var stream = pdfInterviewGenerator.Generate(interview.Id, PdfView.Interviewer)
                    ?? throw new ArgumentException($"Failed to generate pdf for interview {interview.Id}");
                using var buffer = new MemoryStream();
                await stream.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);
                attachments.Add(new EmailAttachment
                {
                    Filename = interview.GetInterviewKey() + ".pdf",
                    ContentType = "application/pdf",
                    Content = buffer.ToArray(),
                    Disposition = EmailAttachmentDisposition.Attachment
                });
            }

            var template = config.GetEmailTemplate(EmailTextTemplateType.CompleteInterviewEmail);
            var emailParamsId = SaveEmailForOnlineAccess(senderInfo, template, questionnaire, interview, assignment);
            var htmlParams = CreateEmailParameters(EmailContentTextMode.Html, senderInfo, template,
                questionnaire, interview, attachments, emailParamsId, assignment);
            var html = await webInterviewEmailRenderer.RenderHtmlEmail(htmlParams).ConfigureAwait(false);
            var textParams = CreateEmailParameters(EmailContentTextMode.Text, senderInfo, template,
                questionnaire, interview, attachments, emailParamsId, assignment);
            var text = await webInterviewEmailRenderer.RenderTextEmail(textParams).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            return new PreparedInterviewCompletedEmail
            {
                Address = assignment.Email,
                Subject = textParams.Subject,
                Html = html,
                Text = text,
                Attachments = attachments
            };
        }

        private static EmailParameters CreateEmailParameters(EmailContentTextMode textMode,
            ISenderInformation senderInfo, WebInterviewEmailTemplate emailTemplate,
            IQuestionnaire questionnaire, IStatefulInterview interview, List<EmailAttachment> attachments,
            string emailParamsId, Assignment assignment)
        {
            var content = new EmailContent(emailTemplate, questionnaire.Title, null, null)
            {
                AttachmentMode = EmailContentAttachmentMode.InlineAttachment,
                TextMode = textMode
            };
            content.RenderInterviewData(interview, questionnaire);
            attachments.AddRange(content.Attachments);
            return CreateParameters(content, senderInfo, questionnaire.Title, emailParamsId, assignment.Id);
        }

        private string SaveEmailForOnlineAccess(ISenderInformation senderInfo, WebInterviewEmailTemplate template,
            IQuestionnaire questionnaire, IStatefulInterview interview, Assignment assignment)
        {
            var content = new EmailContent(template, questionnaire.Title, null, null)
            {
                AttachmentMode = EmailContentAttachmentMode.Base64String
            };
            content.RenderInterviewData(interview, questionnaire);
            var id = $"{Guid.NewGuid():N}-{assignment.Id}-Complete";
            emailParamsStorage.Store(CreateParameters(content, senderInfo, questionnaire.Title, id, assignment.Id), id);
            return id;
        }

        private static EmailParameters CreateParameters(EmailContent content, ISenderInformation senderInfo,
            string surveyName, string id, int assignmentId) => new EmailParameters
        {
            Id = id,
            AssignmentId = assignmentId,
            Subject = content.Subject,
            LinkText = content.LinkText,
            MainText = content.MainText,
            PasswordDescription = content.PasswordDescription,
            Password = null,
            Address = senderInfo.Address,
            SurveyName = surveyName,
            SenderName = senderInfo.SenderName,
            Link = null
        };
    }
}

