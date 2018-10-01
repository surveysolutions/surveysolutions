using System;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization
{
    public class SupervisorUpdateApplication : UpdateApplication
    {
        private readonly ISupervisorSettings interviewerSettings;

        public SupervisorUpdateApplication(int sortOrder, 
            ISynchronizationService synchronizationService, 
            ILogger logger, 
            ISupervisorSettings interviewerSettings,
            ITabletDiagnosticService diagnosticService) : 
            base(sortOrder, synchronizationService, diagnosticService, logger)
        {
            this.interviewerSettings = interviewerSettings ?? throw new ArgumentNullException(nameof(interviewerSettings));
        }

        protected override int GetApplicationVersionCode()
        {
            return interviewerSettings.GetApplicationVersionCode();
        }
    }
}
