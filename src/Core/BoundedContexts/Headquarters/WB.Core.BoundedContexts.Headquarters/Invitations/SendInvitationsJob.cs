using System;
using System.Diagnostics;
using System.Threading.Tasks;
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
        private readonly IInvitationMailingService invitationMailingService;

        public SendInvitationsJob(
            ILogger logger, 
            IInvitationService invitationService, 
            IEmailService emailService, 
            IInvitationMailingService invitationMailingService)
        {
            this.logger = logger;
            this.invitationService = invitationService;
            this.emailService = emailService;
            this.invitationMailingService = invitationMailingService;
        }

        public async Task Execute(IJobExecutionContext context)
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

                invitationService.StartEmailDistribution();
                var cancellationToken = invitationService.GetCancellationToken();
                var sw = new Stopwatch();
                sw.Start();

                var invitationIdsToSend = invitationService.GetInvitationIdsToSend(status.QuestionnaireIdentity);

                foreach (var invitationId in invitationIdsToSend)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    Invitation invitation = invitationService.GetInvitation(invitationId);
                    var address = invitation.Assignment.Email;
                    try
                    {
                        await invitationMailingService.SendInvitationAsync(invitationId, invitation.Assignment);
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
        public SendInvitationsTask(IScheduler scheduler) 
            : base(scheduler, "Send invitations", typeof(SendInvitationsJob)) { }
    }
}
