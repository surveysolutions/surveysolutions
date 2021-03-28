using System;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.Core.BoundedContexts.Interviewer.Synchronization
{
    public class InterviewerUpdateAppSettings : UpdateAppSettings
    { 
        private readonly IInterviewerSettings interviewerSettings;

        public InterviewerUpdateAppSettings(int sortOrder, 
            ISynchronizationService synchronizationService,
            ILogger logger, 
            IInterviewerSettings interviewerSettings) : base(synchronizationService, 
            logger, sortOrder)
        {
            this.interviewerSettings = interviewerSettings ?? throw new ArgumentNullException(nameof(interviewerSettings));
        }

        public override async Task ExecuteAsync()
        {
            await base.ExecuteAsync();

            var tabletSettings = await this.synchronizationService.GetTabletSettings(Context.CancellationToken);
            interviewerSettings.SetPartialSynchronizationEnabled(tabletSettings.PartialSynchronizationEnabled);
            interviewerSettings.SetQuestionnaireInWebMode(tabletSettings.QuestionnairesInWebMode);
            interviewerSettings.SetWebInterviewUrlTemplate(tabletSettings.WebInterviewUrlTemplate);
        }


        protected override void UpdateNotificationsSetting(bool notificationsEnabled)
        {
            interviewerSettings.SetNotifications(notificationsEnabled);
        }
    }
}
