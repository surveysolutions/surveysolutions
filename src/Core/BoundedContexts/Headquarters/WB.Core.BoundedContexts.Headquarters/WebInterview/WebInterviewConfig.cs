using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.WebInterview
{
    public class WebInterviewConfig
    {
        public WebInterviewConfig()
        {
            this.CustomMessages = new Dictionary<WebInterviewUserMessages, string>();
            this.EmailTemplates = new Dictionary<EmailTextTemplateType, EmailTextTemplate>();
        }

        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool SingleResponse { get; set; }

        public QuestionnaireIdentity QuestionnaireId { get; set; }
        public bool Started { get; set; }
        public bool UseCaptcha { get; set; }
        public bool EmailOnComplete { get; set; }
        public bool AttachAnswersInEmail { get; set; }
        public bool AllowSwitchToCawiForInterviewer { get;set; }

        public Dictionary<WebInterviewUserMessages, string> CustomMessages { get; set; }

        public static Dictionary<WebInterviewUserMessages, string> DefaultMessages =>
            new Dictionary<WebInterviewUserMessages, string>
            {
                { WebInterviewUserMessages.FinishInterview, WebInterviewResources.FinishInterviewText },
                { WebInterviewUserMessages.Invitation,      WebInterviewResources.InvitationText },
                { WebInterviewUserMessages.ResumeInvitation,WebInterviewResources.Resume_InvitationText },
                { WebInterviewUserMessages.ResumeWelcome,   WebInterviewResources.Resume_WelcomeText },
                { WebInterviewUserMessages.SurveyName,      WebInterviewResources.SurveyFormatText },
                { WebInterviewUserMessages.WebSurveyHeader, WebInterviewResources.WebSurvey },
                { WebInterviewUserMessages.WelcomeText,     WebInterviewResources.WelcomeText },
                { WebInterviewUserMessages.CompleteNoteToSupervisor,     WebInterviewResources.NoteForSupervisor},
                { WebInterviewUserMessages.CompleteButton,  WebInterviewResources.Complete},
                { WebInterviewUserMessages.StartNewButton,  WebInterviewResources.StartNewInterview},
                { WebInterviewUserMessages.ResumeButton,    WebInterviewResources.ResumeInterview},
                { WebInterviewUserMessages.LinkWelcome,    WebInterviewResources.LinkWelcome},
                { WebInterviewUserMessages.LinkInvitation,    WebInterviewResources.LinkInvitation},

            };

        public Dictionary<EmailTextTemplateType, EmailTextTemplate> EmailTemplates { get; set; }

        public static Dictionary<EmailTextTemplateType, EmailTextTemplate> DefaultEmailTemplates =>
        new Dictionary<EmailTextTemplateType, EmailTextTemplate>()
        {
            { EmailTextTemplateType.InvitationTemplate, new EmailTextTemplate(EmailTemplateTexts.InvitationTemplate.Subject, EmailTemplateTexts.InvitationTemplate.Message, EmailTemplateTexts.InvitationTemplate.PasswordDescription, EmailTemplateTexts.InvitationTemplate.LinkText) },
            { EmailTextTemplateType.ResumeTemplate, new EmailTextTemplate(EmailTemplateTexts.ResumeTemplate.Subject, EmailTemplateTexts.ResumeTemplate.Message, EmailTemplateTexts.ResumeTemplate.PasswordDescription, EmailTemplateTexts.ResumeTemplate.LinkText) },
            { EmailTextTemplateType.Reminder_NoResponse, new EmailTextTemplate(EmailTemplateTexts.Reminder_NoResponse.Subject, EmailTemplateTexts.Reminder_NoResponse.Message, EmailTemplateTexts.Reminder_NoResponse.PasswordDescription, EmailTemplateTexts.Reminder_NoResponse.LinkText) },
            { EmailTextTemplateType.Reminder_PartialResponse, new EmailTextTemplate(EmailTemplateTexts.Reminder_PartialResponse.Subject, EmailTemplateTexts.Reminder_PartialResponse.Message, EmailTemplateTexts.Reminder_PartialResponse.PasswordDescription, EmailTemplateTexts.Reminder_PartialResponse.LinkText) },
            { EmailTextTemplateType.RejectEmail, new EmailTextTemplate(EmailTemplateTexts.RejectEmail.Subject, EmailTemplateTexts.RejectEmail.Message, EmailTemplateTexts.RejectEmail.PasswordDescription, EmailTemplateTexts.RejectEmail.LinkText) },
            { EmailTextTemplateType.CompleteInterviewEmail, new EmailTextTemplate(EmailTemplateTexts.CompleteEmail.Subject, EmailTemplateTexts.CompleteEmail.Message, EmailTemplateTexts.CompleteEmail.PasswordDescription, EmailTemplateTexts.CompleteEmail.LinkText) },
        };

        public int? ReminderAfterDaysIfNoResponse { get; set; }
        public int? ReminderAfterDaysIfPartialResponse { get; set; }

        public virtual WebInterviewEmailTemplate GetEmailTemplate(EmailTextTemplateType type)
        {
            var template = EmailTemplates.ContainsKey(type)
                ? EmailTemplates[type]
                : DefaultEmailTemplates[type];

            return new WebInterviewEmailTemplate(template.Subject, template.Message, template.PasswordDescription, template.LinkText);
        }
    }

    public class EmailTextTemplate
    {
        protected EmailTextTemplate() { }

        public EmailTextTemplate(string subject, string message, string passwordDescription, string linkText)
        {
            Subject = subject;
            Message = message;
            PasswordDescription = passwordDescription;
            LinkText = linkText;
        }

        public string Subject { get; set; }
        public string Message { get; set; }
        public string PasswordDescription { get; set; }
        public string LinkText { get; set; }
    }

    public enum EmailTextTemplateType
    {
        InvitationTemplate,
        Reminder_NoResponse,
        Reminder_PartialResponse,
        RejectEmail,
        ResumeTemplate,
        CompleteInterviewEmail,
    }
}
