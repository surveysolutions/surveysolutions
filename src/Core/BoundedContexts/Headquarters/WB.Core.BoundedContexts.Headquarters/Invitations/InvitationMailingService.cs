using System;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.EmailProviders;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public class InvitationMailingService : IInvitationMailingService
    {
        private readonly IInvitationService invitationService;
        private readonly IEmailService emailService;
        private readonly IWebInterviewConfigProvider webInterviewConfigProvider;
        private readonly IPlainKeyValueStorage<EmailParameters> emailParamsStorage;
        private readonly IWebInterviewEmailRenderer webInterviewEmailRenderer;
        private readonly IWebInterviewLinkProvider linkProvider;

        public InvitationMailingService(
            IInvitationService invitationService, 
            IEmailService emailService,
            IWebInterviewConfigProvider webInterviewConfigProvider, 
            IPlainKeyValueStorage<EmailParameters> emailParamsStorage,
            IWebInterviewEmailRenderer webInterviewEmailRenderer,
            IWebInterviewLinkProvider linkProvider)
        {
            this.invitationService = invitationService;
            this.emailService = emailService;
            this.webInterviewConfigProvider = webInterviewConfigProvider;
            this.emailParamsStorage = emailParamsStorage;
            this.webInterviewEmailRenderer = webInterviewEmailRenderer;
            this.linkProvider = linkProvider;
        }

        public async Task<string> SendInvitationAsync(int invitationId, Assignment assignment, string email = null)
        {
            Invitation invitation = invitationService.GetInvitation(invitationId);
            var link = this.linkProvider.WebInterviewStartLink(invitation);
            return await SendEmailByTemplate(invitation, assignment, email, EmailTextTemplateType.InvitationTemplate, link);
        }
        
        public async Task<string> SendResumeAsync(int invitationId, Assignment assignment, string email)
        {
            Invitation invitation = invitationService.GetInvitation(invitationId);
            var link = this.linkProvider.WebInterviewContinueLink(invitation);
            return await SendEmailByTemplate(invitation, assignment, email, EmailTextTemplateType.ResumeTemplate, link);
        }

        private async Task<string> SendEmailByTemplate(Invitation invitation, Assignment assignment, string email, 
            EmailTextTemplateType emailTemplateType, string link)
        {
            WebInterviewConfig webInterviewConfig = webInterviewConfigProvider.Get(assignment.QuestionnaireId);
            var emailTemplate = webInterviewConfig.GetEmailTemplate(emailTemplateType);

            var questionnaireTitle = assignment.Questionnaire.Title;

            var senderInfo = emailService.GetSenderInfo();

            var password = assignment.Password;
            var address = email ?? assignment.Email;

            var emailContent = new EmailContent(emailTemplate, questionnaireTitle, link, password);

            var emailParamsId = $"{Guid.NewGuid().FormatGuid()}-{invitation.Id}";
            var emailParams = new EmailParameters
            {
                Id = emailParamsId,
                AssignmentId = invitation.AssignmentId,
                InvitationId = invitation.Id,
                Subject = emailContent.Subject,
                LinkText = emailContent.LinkText,
                MainText = emailContent.MainText,
                PasswordDescription = emailContent.PasswordDescription,
                Password = password,
                Address = senderInfo.Address,
                SurveyName = questionnaireTitle,
                SenderName = senderInfo.SenderName,
                Link = link
            };

            emailParamsStorage.Store(emailParams, emailParamsId);

            var interviewEmail = await webInterviewEmailRenderer.RenderEmail(emailParams);
            var emailId = await emailService.SendEmailAsync(address, emailParams.Subject, interviewEmail.MessageHtml.Trim(), interviewEmail.MessageText.Trim());
            return emailId;
        }
    }
}
