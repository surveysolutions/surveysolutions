using System;
using System.Diagnostics;
using System.Threading;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.EmailProviders;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    [DisallowConcurrentExecution]
    public class SendInvitationsJob : IJob
    {
        private readonly ILogger logger;
        private readonly IInvitationService invitationService;
        private readonly IEmailService emailService;

        public SendInvitationsJob(
            ILogger logger, 
            IInvitationService invitationService, 
            IEmailService emailService)
        {
            this.logger = logger;
            this.invitationService = invitationService;
            this.emailService = emailService;
        }

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                InvitationDistributionStatus status = invitationService.GetEmailDistributionStatus();

                if (status == null)
                    return;

                if (status.Status > InvitationProcessStatus.Started)
                    return;

                this.logger.Debug("Invitations distribution job: Started");

                invitationService.StartEmailDistribution();
                CancellationToken cancellationToken = invitationService.GetCancellationToken() ?? throw new Exception("Cancellation token was requested for not started process");
                var sw = new Stopwatch();
                sw.Start();

                var invitationIdsToSend = invitationService.GetInvitationIdsToSend(status.QuestionnaireIdentity);
                // read email template

                var emailTemplate = new EmailTemplate(
                    "Invitation to take part in %SURVEYNAME%",
                    @"Welcome to %SURVEYNAME%! 
To take the survey click on the following link: %SURVEYLINK% and enter your password: %PASSWORD% 
Thank you for your cooperation!");

                var surveyName = "Hello World Survey";

                foreach (var invitationId in invitationIdsToSend)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    Invitation invitation = invitationService.GetInvitation(invitationId);
                    var password = invitation.Assignment.Password;
                    var address = invitation.Assignment.Email;

                    var link = "https://hell0.world";

                    var hasPassword = !string.IsNullOrWhiteSpace(password);
                    if (!hasPassword && emailTemplate.HasPassword)
                    {
                        // can't be sent
                        invitationService.InvitationWasNotSent(invitationId, "Assignment doesn't have password, but template requires password");
                        continue;
                    }

                    if (hasPassword && !emailTemplate.HasPassword)
                    {
                        // can't be sent
                        invitationService.InvitationWasNotSent(invitationId, "Assignment has password, but template doesn't %PASSWORD%.");
                        continue;
                    }

                    var email = UserEmail.FromTemplate(emailTemplate)
                        .SubstitutePassword(password)
                        .SubstituteLink(link)
                        .SubstituteSurveyName(surveyName);
                    
                    try
                    {
                        var emailId = emailService.SendEmailAsync(address, email.Subject, email.Message, email.Message).Result;
                        invitationService.MarkInvitationAsSent(invitationId, emailId);
                    }
                    catch (EmailServiceException e)
                    {
                        invitationService.InvitationWasNotSent(invitationId, e.Message);
                    }
                }
               
                sw.Stop();
                
                invitationService.StopEmailDistribution();

                this.logger.Debug($"Invitations distribution job: Finished. Elapsed time: {sw.Elapsed}");
            }
            catch (OperationCanceledException)
            {
                invitationService.EmailDistributionFailed();
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
