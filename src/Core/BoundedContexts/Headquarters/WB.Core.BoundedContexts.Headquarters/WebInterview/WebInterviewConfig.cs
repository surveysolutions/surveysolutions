using System.Collections.Generic;
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

        public QuestionnaireIdentity QuestionnaireId { get; set; }
        public bool Started { get; set; }
        public bool UseCaptcha { get; set; }
        public Dictionary<WebInterviewUserMessages, string> CustomMessages { get; set; }

        public static Dictionary<WebInterviewUserMessages, string> DefaultMessages => 
            new Dictionary<WebInterviewUserMessages, string> 
            {
                { WebInterviewUserMessages.FinishInterview, Enumerator.Native.Resources.WebInterview.FinishInterviewText },
                { WebInterviewUserMessages.Invitation,      Enumerator.Native.Resources.WebInterview.InvitationText },
                { WebInterviewUserMessages.ResumeInvitation,Enumerator.Native.Resources.WebInterview.Resume_InvitationText },
                { WebInterviewUserMessages.ResumeWelcome,   Enumerator.Native.Resources.WebInterview.Resume_WelcomeText },
                { WebInterviewUserMessages.SurveyName,      Enumerator.Native.Resources.WebInterview.SurveyFormatText },
                { WebInterviewUserMessages.WebSurveyHeader, Enumerator.Native.Resources.WebInterview.WebSurvey },
                { WebInterviewUserMessages.WelcomeText,     Enumerator.Native.Resources.WebInterview.WelcomeText }
            };

        public Dictionary<EmailTextTemplateType, EmailTextTemplate> EmailTemplates { get; set; }

        public static Dictionary<EmailTextTemplateType, EmailTextTemplate> DefaultEmailTemplates =>
        new Dictionary<EmailTextTemplateType, EmailTextTemplate>()
        {
            { EmailTextTemplateType.InvitationTemplate, new EmailTextTemplate(Enumerator.Native.Resources.WebInterview.Email_InvitationTemplate_Subject, Enumerator.Native.Resources.WebInterview.Email_InvitationTemplate_Message) },
            { EmailTextTemplateType.Reminder_NoResponse, new EmailTextTemplate(Enumerator.Native.Resources.WebInterview.Email_NoResponse_Subject, Enumerator.Native.Resources.WebInterview.Email_NoResponse_Message) },
            { EmailTextTemplateType.Reminder_PartialResponse, new EmailTextTemplate(Enumerator.Native.Resources.WebInterview.Email_PartialResponse_Subject, Enumerator.Native.Resources.WebInterview.Email_PartialResponse_Message) },
            { EmailTextTemplateType.RejectEmail, new EmailTextTemplate(Enumerator.Native.Resources.WebInterview.Email_RejectEmail_Subject, Enumerator.Native.Resources.WebInterview.Email_RejectEmail_Message) }
        };

        public int ReminderAfterDaysIfNoResponse { get; set; }
        public int ReminderAfterDaysIfPartialResponse { get; set; }

        public string GetEmailSubject(EmailTextTemplateType type)
        {
            if (EmailTemplates.ContainsKey(type))
                return EmailTemplates[type].Subject;

            return DefaultEmailTemplates[type].Subject;
        }

        public string GetEmailMessage(EmailTextTemplateType type)
        {
            if (EmailTemplates.ContainsKey(type))
                return EmailTemplates[type].Message;

            return DefaultEmailTemplates[type].Message;
        }
    }

    public class EmailTextTemplate
    {
        public EmailTextTemplate() { }

        public EmailTextTemplate(string subject, string message)
        {
            Subject = subject;
            Message = message;
        }

        public string Subject { get; set; }
        public string Message { get; set; }
    }

    public enum EmailTextTemplateType
    {
        InvitationTemplate,
        Reminder_NoResponse,
        Reminder_PartialResponse,
        RejectEmail,
    }
}
