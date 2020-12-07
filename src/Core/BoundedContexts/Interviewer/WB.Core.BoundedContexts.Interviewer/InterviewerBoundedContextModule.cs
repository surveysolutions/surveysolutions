using System.Diagnostics.CodeAnalysis;
using WB.Core.BoundedContexts.Interviewer.Synchronization;
using WB.Core.Infrastructure.Modularity;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;

namespace WB.Core.BoundedContexts.Interviewer
{
    [ExcludeFromCodeCoverage]
    public class InterviewerBoundedContextModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.BindWithConstructorArgument<ISynchronizationStep, OfflineInterviewerUpdateApplication>("sortOrder", 0);
            registry.BindWithConstructorArgument<ISynchronizationStep, UpdateEncryptionKey>("sortOrder", 10);
            registry.BindWithConstructorArgument<ISynchronizationStep, InterviewerUpdateAppSettings>("sortOrder", 20);
            registry.BindWithConstructorArgument<ISynchronizationStep, DownloadHQChangesForInterview>("sortOrder", 30);
            registry.BindWithConstructorArgument<ISynchronizationStep, UploadCalendarEvents>("sortOrder", 35);
            registry.BindWithConstructorArgument<ISynchronizationStep, InterviewerUploadInterviews>("sortOrder", 40);
            registry.BindWithConstructorArgument<ISynchronizationStep, SynchronizeAssignments>("sortOrder", 50);
            registry.BindWithConstructorArgument<ISynchronizationStep, RemoveObsoleteQuestionnaires>("sortOrder", 70);
            registry.BindWithConstructorArgument<ISynchronizationStep, InterviewerDownloadInterviews>("sortOrder", 80);
            registry.BindWithConstructorArgument<ISynchronizationStep, DownloadCalendarEvents>("sortOrder", 85);
            registry.BindWithConstructorArgument<ISynchronizationStep, SynchronizeLogo>("sortOrder", 90);
            registry.BindWithConstructorArgument<ISynchronizationStep, SynchronizeAuditLog>("sortOrder", 100);
            registry.BindWithConstructorArgument<ISynchronizationStep, IUpdateApplicationSynchronizationStep, InterviewerUpdateApplication>("sortOrder", 70);
        }
    }
}
