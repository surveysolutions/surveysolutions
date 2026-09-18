using System;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.Core.BoundedContexts.Interviewer.Synchronization
{
    public class InterviewerUpdateApplication : UpdateApplication
    {
        private readonly IInterviewerSettings interviewerSettings;

        public InterviewerUpdateApplication(int sortOrder, 
            ISynchronizationService synchronizationService,
            ILogger logger, 
            IInterviewerSettings interviewerSettings,
            ITabletDiagnosticService diagnosticService) : base(sortOrder, synchronizationService, diagnosticService,
            logger)
        {
            this.interviewerSettings = interviewerSettings ?? throw new ArgumentNullException(nameof(interviewerSettings));
        }

        public override Task ExecuteAsync()
        {
            if (!this.interviewerSettings.AllowSyncWithHq)
                return Task.CompletedTask;

            return base.ExecuteAsync();
        }

        protected override int GetApplicationVersionCode()
        {
            return interviewerSettings.GetApplicationVersionCode();
        }
    }
}
