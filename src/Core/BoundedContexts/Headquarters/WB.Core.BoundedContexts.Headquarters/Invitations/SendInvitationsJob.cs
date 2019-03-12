using System;
using System.Diagnostics;
using System.Net.Http;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.EmailProviders;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    [DisallowConcurrentExecution]
    public class SendInvitationsJob : IJob
    {
        private readonly ILogger logger;
        private readonly IInvitationService invitationService;
        private readonly IEmailService emailService;
        private readonly IWebInterviewConfigProvider webInterviewConfigProvider;
        private readonly IPlainKeyValueStorage<EmailParameters> emailParamsStorage;

        public SendInvitationsJob(
            ILogger logger, 
            IInvitationService invitationService, 
            IEmailService emailService,
            IWebInterviewConfigProvider webInterviewConfigProvider, 
            IPlainKeyValueStorage<EmailParameters> emailParamsStorage)
        {
            this.logger = logger;
            this.invitationService = invitationService;
            this.emailService = emailService;
            this.webInterviewConfigProvider = webInterviewConfigProvider;
            this.emailParamsStorage = emailParamsStorage;
        }

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                if (!emailService.IsConfigured())
                    return;

                InvitationDistributionStatus status = invitationService.GetEmailDistributionStatus();

                if (status == null)
                    return;

                if (status.Status > InvitationProcessStatus.InProgress)
                    return;

                this.logger.Debug("Invitations distribution job: Started");

                WebInterviewConfig webInterviewConfig = webInterviewConfigProvider.Get(status.QuestionnaireIdentity);
                
                var senderInfo = emailService.GetSenderInfo();

                invitationService.StartEmailDistribution();
                var cancellationToken = invitationService.GetCancellationToken();
                var sw = new Stopwatch();
                sw.Start();

                var invitationIdsToSend = invitationService.GetInvitationIdsToSend(status.QuestionnaireIdentity);

                var emailTemplate = webInterviewConfig.GetEmailTemplate(EmailTextTemplateType.InvitationTemplate);

                var surveyName = status.QuestionnaireTitle;

                foreach (var invitationId in invitationIdsToSend)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    Invitation invitation = invitationService.GetInvitation(invitationId);

                    var password = invitation.Assignment.Password;
                    var address = invitation.Assignment.Email;
                    var link = $"{webInterviewConfig.BaseUrl}/{invitation.Token}/Start";

                    var emailParamsId = $"{Guid.NewGuid().FormatGuid()}-{invitationId}";
                    var emailParams = new EmailParameters
                    {
                        AssignmentId = invitation.AssignmentId,
                        InvitationId = invitation.Id,
                        Subject = emailTemplate.Subject
                            .Replace(WebInterviewEmailTemplate.SurveyName, surveyName),
                        LinkText = emailTemplate.LinkText,
                        MainText = emailTemplate.MainText
                            .Replace(WebInterviewEmailTemplate.SurveyName, surveyName),
                        PasswordDescription = emailTemplate.PasswordDescription,
                        Password = password,
                        Address = senderInfo.Address,
                        SurveyName = surveyName,
                        SenderName = senderInfo.SenderName,
                        Link = link
                    };
                    emailParamsStorage.Store(emailParams, emailParamsId);

                    var client = new HttpClient{};
                    var htmlMessage = client.GetStringAsync($"{webInterviewConfig.BaseUrl}/WebEmails/Html/{emailParamsId}").Result ?? string.Empty;
                    var textMessage = client.GetStringAsync($"{webInterviewConfig.BaseUrl}/WebEmails/Text/{emailParamsId}/").Result ?? string.Empty;

                    try
                    {
                        var emailId = emailService.SendEmailAsync(address, emailParams.Subject, htmlMessage.Trim(), textMessage.Trim()).Result;
                        invitationService.MarkInvitationAsSent(invitationId, emailId);
                    }
                    catch (EmailServiceException e)
                    {
                        invitationService.InvitationWasNotSent(invitationId, invitation.AssignmentId, address, e.Message);
                    }
                }
               
                sw.Stop();
                
                invitationService.CompleteEmailDistribution();

                this.logger.Debug($"Invitations distribution job: Finished. Elapsed time: {sw.Elapsed}");
            }
            catch (OperationCanceledException)
            {
                invitationService.EmailDistributionCanceled();
                this.logger.Error($"Invitations distribution job: CANCELED.");
            }
            catch (Exception ex)
            {
                invitationService.EmailDistributionFailed();
                this.logger.Error($"Invitations distribution job: FAILED. Reason: {ex.Message} ", ex);
            }
        }
    }

    public class SendInvitationsTask : BaseTask
    {
        public SendInvitationsTask(IScheduler scheduler) : base(scheduler, "Send invitations", typeof(SendInvitationsJob)) { }
    }
}
