#nullable enable

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.EmailProviders;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.Infrastructure.Domain;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    [DisallowConcurrentExecution]
    public class SendInterviewCompletedJob : IJob
    {
        private const int BatchSize = 100;
        private static readonly TimeSpan RunBudget = TimeSpan.FromMinutes(1);
        private readonly ILogger<SendInterviewCompletedJob> logger;
        private readonly IEmailService emailService;
        private readonly IInScopeExecutor<ICompletedEmailsQueue> queueExecutor;
        private readonly IInScopeExecutor<IInterviewCompletedEmailBuilder> preparationExecutor;

        public SendInterviewCompletedJob(
            ILogger<SendInterviewCompletedJob> logger,
            IEmailService emailService,
            IInScopeExecutor<ICompletedEmailsQueue> queueExecutor,
            IInScopeExecutor<IInterviewCompletedEmailBuilder> preparationExecutor)
        {
            this.logger = logger;
            this.emailService = emailService;
            this.queueExecutor = queueExecutor;
            this.preparationExecutor = preparationExecutor;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var cancellationToken = context.CancellationToken;
                cancellationToken.ThrowIfCancellationRequested();
                if (!emailService.IsConfigured())
                    return;

                var sw = Stopwatch.StartNew();
                var senderInfo = emailService.GetSenderInfo();
                var interviewIds = queueExecutor.Execute(queue => queue.GetInterviewIdsForSend(BatchSize));

                // A fixed snapshot bounds the run even when new completions arrive continuously.
                foreach (var interviewId in interviewIds)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (sw.Elapsed >= RunBudget)
                        break;

                    try
                    {
                        var email = await preparationExecutor.ExecuteAsync(builder =>
                            builder.PrepareAsync(interviewId, senderInfo, cancellationToken)).ConfigureAwait(false);
                        cancellationToken.ThrowIfCancellationRequested();

                        // The preparation scope is already disposed, including its transaction.
                        if (email != null)
                        {
                            await emailService.SendEmailAsync(email.Address, email.Subject, email.Html,
                                email.Text, email.Attachments).ConfigureAwait(false);
                        }

                        // Acknowledge a successful send even if cancellation arrived during SMTP.
                        queueExecutor.Execute(queue => queue.Remove(interviewId));
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        logger.LogWarning(e, "Completed email for {interviewId} failed", interviewId);
                        // Use a fresh scope; the failed preparation may have invalidated its session.
                        queueExecutor.Execute(queue => queue.MarkAsFailedToSend(interviewId));
                    }
                }
            }
            catch (OperationCanceledException)
            {
                this.logger.LogWarning("Send completed emails job: CANCELED");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Send completed emails job: FAILED");
            }
        }

    }

    public class SendInterviewCompletedTask : BaseTask
    {
        public SendInterviewCompletedTask(ISchedulerFactory schedulerFactory) 
            : base(schedulerFactory, "Send interview completed emails", typeof(SendInterviewCompletedJob)) { }
    }
}
