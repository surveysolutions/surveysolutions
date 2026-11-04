using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.EmailProviders;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.Infrastructure.Domain;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;


namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    [DisallowConcurrentExecution]
    public class SendRemindersJob : IJob
    {
        private readonly ILogger<SendRemindersJob> logger;
        private readonly IInvitationService invitationService;
        private readonly IEmailService emailService;
        private readonly IWebInterviewConfigProvider webInterviewConfigProvider;
        private readonly IRootScopeExecutor rootScopeExecutor;
       
        public SendRemindersJob(
            ILogger<SendRemindersJob> logger, 
            IInvitationService invitationService, 
            IEmailService emailService,
            IWebInterviewConfigProvider webInterviewConfigProvider,
            IRootScopeExecutor rootScopeExecutor)
        {
            this.logger = logger;
            this.invitationService = invitationService;
            this.emailService = emailService;
            this.webInterviewConfigProvider = webInterviewConfigProvider;
            this.rootScopeExecutor = rootScopeExecutor;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                if (!emailService.IsConfigured())
                    return;

                var questionnaires = invitationService.GetQuestionnairesWithInvitations().ToList();
                
                var sw = new Stopwatch();
                sw.Start();

                foreach (QuestionnaireBrowseItem questionnaire in questionnaires)
                {
                    var questionnaireIdentity = questionnaire.Identity();
                    WebInterviewConfig webInterviewConfig = webInterviewConfigProvider.Get(questionnaireIdentity);
                    
                    if(webInterviewConfig == null || !webInterviewConfig.Started)
                        continue;

                    if (webInterviewConfig.ReminderAfterDaysIfNoResponse.HasValue)
                    {
                        await SendRemindersAsync(questionnaireIdentity, questionnaire.Title,
                            EmailTextTemplateType.Reminder_NoResponse,
                            webInterviewConfig.ReminderAfterDaysIfNoResponse.Value).ConfigureAwait(false);
                    }

                    if (webInterviewConfig.ReminderAfterDaysIfPartialResponse.HasValue)
                    {
                        await SendRemindersAsync(questionnaireIdentity, questionnaire.Title,
                            EmailTextTemplateType.Reminder_PartialResponse,
                            webInterviewConfig.ReminderAfterDaysIfPartialResponse.Value).ConfigureAwait(false);
                    }

                    await SendRemindersAsync(questionnaireIdentity, questionnaire.Title,
                        EmailTextTemplateType.RejectEmail, 0).ConfigureAwait(false);
                }

                sw.Stop();
            }
            catch (OperationCanceledException)
            {
                this.logger.LogWarning("Reminders distribution job: CANCELED");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Reminders distribution job for workspace {context.Get("workspace")}: FAILED");
            }
        }

        private async Task SendRemindersAsync(QuestionnaireIdentity questionnaireIdentity,
            string questionnaireTitle, EmailTextTemplateType reminderType, int thresholdDays)
        {
            try
            {
                await rootScopeExecutor.ExecuteAsync(locator =>
                    locator.GetInstance<IReminderEmailSender>().SendRemindersAsync(questionnaireIdentity,
                        questionnaireTitle, reminderType, thresholdDays)).ConfigureAwait(false);
            }
            catch (ReminderDeliveryInProgressException)
            {
                logger.LogDebug("Skipping reminder batch {QuestionnaireIdentity}/{ReminderType} because delivery is already in progress.",
                    questionnaireIdentity, reminderType);
            }
        }
    }

    public class SendRemindersTask : BaseTask
    {
        public SendRemindersTask(ISchedulerFactory schedulerFactory) : base(schedulerFactory, "Send reminders", typeof(SendRemindersJob)) { }
    }
}
