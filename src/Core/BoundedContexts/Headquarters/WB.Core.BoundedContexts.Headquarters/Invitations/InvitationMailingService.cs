﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.EmailProviders;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
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

        public InvitationMailingService(
            IInvitationService invitationService, 
            IEmailService emailService,
            IWebInterviewConfigProvider webInterviewConfigProvider, 
            IPlainKeyValueStorage<EmailParameters> emailParamsStorage,
            IWebInterviewEmailRenderer webInterviewEmailRenderer)
        {
            this.invitationService = invitationService;
            this.emailService = emailService;
            this.webInterviewConfigProvider = webInterviewConfigProvider;
            this.emailParamsStorage = emailParamsStorage;
            this.webInterviewEmailRenderer = webInterviewEmailRenderer;
        }

        public async Task SendInvitationAsync(int invitationId, Assignment assignment, string email = null)
        {
            Invitation invitation = invitationService.GetInvitation(invitationId);
            var link = $"WebInterview/{invitation.Token}/Start";
            await SendEmailByTemplate(invitation, assignment, email, EmailTextTemplateType.InvitationTemplate, link);
        }
        
        public async Task SendResumeAsync(int invitationId, Assignment assignment, string email)
        {
            Invitation invitation = invitationService.GetInvitation(invitationId);
            var link = $"/WebInterview/Continue/{invitation.Token}";
            await SendEmailByTemplate(invitation, assignment, email, EmailTextTemplateType.ResumeTemplate, link);
        }

        private async Task SendEmailByTemplate(Invitation invitation, Assignment assignment, string email, 
            EmailTextTemplateType emailTemplateType, string relativeUri)
        {
            WebInterviewConfig webInterviewConfig = webInterviewConfigProvider.Get(assignment.QuestionnaireId);
            var emailTemplate = webInterviewConfig.GetEmailTemplate(emailTemplateType);

            var questionnaireTitle = assignment.Questionnaire.Title;

            var senderInfo = emailService.GetSenderInfo();

            var password = assignment.Password;
            var address = email ?? assignment.Email;

            var link = new Url(webInterviewConfig.BaseUrl, relativeUri, queryParams: null).ToString();
            var emailContent = new EmailContent(emailTemplate, questionnaireTitle, link, password);

            var emailParamsId = $"{Guid.NewGuid().FormatGuid()}-{invitation.Id}";
            var emailParams = new EmailParameters
            {
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

            var interviewEmail = webInterviewEmailRenderer.RenderEmail(emailParams);
            var emailId = await emailService.SendEmailAsync(address, emailParams.Subject, interviewEmail.MessageHtml.Trim(), interviewEmail.MessageText.Trim());
            invitationService.MarkInvitationAsSent(invitation.Id, emailId);
        }
    }

    public interface IInvitationMailingService
    {
        Task SendInvitationAsync(int invitationId, Assignment assignment, string email = null);
        Task SendResumeAsync(int invitationId, Assignment assignment, string email);
    }
}
