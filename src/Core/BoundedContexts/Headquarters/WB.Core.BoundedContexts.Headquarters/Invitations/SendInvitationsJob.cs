using System;
using System.Diagnostics;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.EmailProviders;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    [DisallowConcurrentExecution]
    public class SendInvitationsJob : IJob
    {
        private readonly ILogger logger;
        private readonly IInvitationService invitationService;
        private readonly IEmailService emailService;
        private readonly IWebInterviewConfigProvider webInterviewConfigProvider;

        public SendInvitationsJob(
            ILogger logger, 
            IInvitationService invitationService, 
            IEmailService emailService,
            IWebInterviewConfigProvider webInterviewConfigProvider)
        {
            this.logger = logger;
            this.invitationService = invitationService;
            this.emailService = emailService;
            this.webInterviewConfigProvider = webInterviewConfigProvider;
        }

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                InvitationDistributionStatus status = invitationService.GetEmailDistributionStatus();

                if (status == null)
                    return;

                if (status.Status > InvitationProcessStatus.InProgress)
                    return;

                this.logger.Debug("Invitations distribution job: Started");

                WebInterviewConfig webInterviewConfig = webInterviewConfigProvider.Get(status.QuestionnaireIdentity);

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

                    var link = $"{status.BaseUrl}/{invitation.Token}/Start";

                    var hasPassword = !string.IsNullOrWhiteSpace(password);

                    var email = PersonalizedWebInterviewEmail.FromTemplate(emailTemplate)
                        .SubstitutePassword(password)
                        .SubstituteLink(link)
                        .SubstituteSurveyName(surveyName);
                    
                    try
                    {
                        var emailId = emailService.SendEmailAsync(address, email.Subject, 
                            hasPassword ? email.MessageWithPasswordHtml : email.MessageHtml, 
                            hasPassword ? email.MessageWithPassword : email.Message).Result;
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
