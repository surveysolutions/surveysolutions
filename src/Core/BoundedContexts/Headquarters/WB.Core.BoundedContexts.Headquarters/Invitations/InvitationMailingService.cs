using System;
using System.Net.Http;
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

        public InvitationMailingService(
            IInvitationService invitationService, 
            IEmailService emailService,
            IWebInterviewConfigProvider webInterviewConfigProvider, 
            IPlainKeyValueStorage<EmailParameters> emailParamsStorage)
        {
            this.invitationService = invitationService;
            this.emailService = emailService;
            this.webInterviewConfigProvider = webInterviewConfigProvider;
            this.emailParamsStorage = emailParamsStorage;
        }

        public async Task SendInvitationAsync(int invitationId, Assignment assignment, string email = null)
        {
            Invitation invitation = invitationService.GetInvitation(invitationId);

            WebInterviewConfig webInterviewConfig = webInterviewConfigProvider.Get(assignment.QuestionnaireId);

            var emailTemplate = webInterviewConfig.GetEmailTemplate(EmailTextTemplateType.InvitationTemplate);

            var questionnaireTitle = assignment.Questionnaire.Title;

            var senderInfo = emailService.GetSenderInfo();

            var password = assignment.Password;
            var address = email ?? assignment.Email;
            var link = $"{webInterviewConfig.BaseUrl}/WebInterview/{invitation.Token}/Start";

            var emailParamsId = $"{Guid.NewGuid().FormatGuid()}-{invitationId}";
            var emailParams = new EmailParameters
            {
                AssignmentId = invitation.AssignmentId,
                InvitationId = invitation.Id,
                Subject = emailTemplate.Subject
                    .Replace(WebInterviewEmailTemplate.SurveyName, questionnaireTitle),
                LinkText = emailTemplate.LinkText,
                MainText = emailTemplate.MainText
                    .Replace(WebInterviewEmailTemplate.SurveyName, questionnaireTitle),
                PasswordDescription = emailTemplate.PasswordDescription,
                Password = password,
                Address = senderInfo.Address,
                SurveyName = questionnaireTitle,
                SenderName = senderInfo.SenderName,
                Link = link
            };
            emailParamsStorage.Store(emailParams, emailParamsId);

            var client = new HttpClient{};
            var htmlMessage = (await client.GetStringAsync($"{webInterviewConfig.BaseUrl}/WebEmails/Html/{emailParamsId}"))?? string.Empty;
            var textMessage = (await client.GetStringAsync($"{webInterviewConfig.BaseUrl}/WebEmails/Text/{emailParamsId}/")) ?? string.Empty;
            
            var emailId = await emailService.SendEmailAsync(address, emailParams.Subject, htmlMessage.Trim(), textMessage.Trim());
            invitationService.MarkInvitationAsSent(invitationId, emailId);
        }
    }

    public interface IInvitationMailingService
    {
        Task SendInvitationAsync(int invitationId, Assignment assignment, string email = null);
    }
}
