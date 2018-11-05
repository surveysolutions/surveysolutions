using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Synchronization;
using WB.Core.BoundedContexts.Interviewer.Synchronization.Steps;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
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
            registry.BindWithConstructorArgument<ISynchronizationStep, UpdateEncryptionKey>("sortOrder", 1);
            registry.BindWithConstructorArgument<ISynchronizationStep, InterviewerUploadInterviews>("sortOrder", 5);
            registry.BindWithConstructorArgument<ISynchronizationStep, SynchronizeAssignments>("sortOrder", 10);
            registry.BindWithConstructorArgument<ISynchronizationStep, CensusQuestionnairesSynchronization>("sortOrder", 20);
            registry.BindWithConstructorArgument<ISynchronizationStep, RemoveObsoleteQuestionnaires>("sortOrder", 30);
            registry.BindWithConstructorArgument<ISynchronizationStep, InterviewerDownloadInterviews>("sortOrder", 40);
            registry.BindWithConstructorArgument<ISynchronizationStep, SynchronizeLogo>("sortOrder", 50);
            registry.BindWithConstructorArgument<ISynchronizationStep, SynchronizeAuditLog>("sortOrder", 60);
            registry.BindWithConstructorArgument<ISynchronizationStep, InterviewerUpdateApplication>("sortOrder", 70);
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            return Task.CompletedTask;
        }
    }
}
