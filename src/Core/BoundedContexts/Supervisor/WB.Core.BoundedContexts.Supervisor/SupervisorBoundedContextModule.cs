using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.Synchronization;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;

namespace WB.Core.BoundedContexts.Supervisor
{
    public class SupervisorBoundedContextModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.BindWithConstructorArgument<ISynchronizationStep, UploadCalendarEvents>("sortOrder", 3);
            registry.BindWithConstructorArgument<ISynchronizationStep, SupervisorUploadInterviews>("sortOrder", 5);
            registry.BindWithConstructorArgument<ISynchronizationStep, UpdateEncryptionKey>("sortOrder", 7);
            registry.BindWithConstructorArgument<ISynchronizationStep, SyncronizeInterviewers>("sortOrder", 10);
            registry.BindWithConstructorArgument<ISynchronizationStep, SynchronizeSupervisor>("sortOrder", 20);
            registry.BindWithConstructorArgument<ISynchronizationStep, SynchronizeAssignments>("sortOrder", 30);
            registry.BindWithConstructorArgument<ISynchronizationStep, DownloadDeletedQuestionnairesList>("sortOrder", 50);
            registry.BindWithConstructorArgument<ISynchronizationStep, SupervisorDownloadInterviews>("sortOrder", 60);
            registry.BindWithConstructorArgument<ISynchronizationStep, UpdateQuestionnaires>("sortOrder", 70);//all questionnaires are downloaded
            registry.BindWithConstructorArgument<ISynchronizationStep, DownloadCalendarEvents>("sortOrder", 73);
            registry.BindWithConstructorArgument<ISynchronizationStep, SynchronizeTechnicalInformation>("sortOrder", 80);
            registry.BindWithConstructorArgument<ISynchronizationStep, SynchronizeLogo>("sortOrder", 90);
            registry.BindWithConstructorArgument<ISynchronizationStep, SynchronizeAuditLog>("sortOrder", 100);
            registry.BindWithConstructorArgument<ISynchronizationStep, IUpdateApplicationSynchronizationStep, SupervisorUpdateApplication>("sortOrder", 110);
            registry.BindWithConstructorArgument<ISynchronizationStep, SupervisorUpdateAppSettings>("sortOrder", 115);
            registry.BindWithConstructorArgument<ISynchronizationStep, DownloadInterviewerApplications>("sortOrder", 120);
        }
    }
}
