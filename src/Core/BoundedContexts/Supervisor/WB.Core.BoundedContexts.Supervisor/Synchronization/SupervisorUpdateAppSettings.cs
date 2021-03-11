using System;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization
{
    public class SupervisorUpdateAppSettings : UpdateAppSettings
    {
        private readonly ISupervisorSettings supervisorSettings;

        public SupervisorUpdateAppSettings(int sortOrder, 
            ISynchronizationService synchronizationService, 
            ILogger logger, 
            ISupervisorSettings interviewerSettings) : 
            base(synchronizationService, logger, sortOrder)
        {
            this.supervisorSettings = interviewerSettings ?? throw new ArgumentNullException(nameof(interviewerSettings));
        }

        protected override void UpdateNotificationsSetting(bool notificationsEnabled)
        {
            supervisorSettings.SetNotifications(notificationsEnabled);
        }
    }
}
