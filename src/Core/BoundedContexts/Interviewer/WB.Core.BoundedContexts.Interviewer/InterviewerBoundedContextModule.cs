using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Synchronization;
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
            registry.Bind<ISynchronizationStep, InterviewerUploadInterviews>(new ConstructorArgument("sortOrder", context => 0));
            registry.Bind<ISynchronizationStep, SynchronizeAssignments>(new ConstructorArgument("sortOrder", context => 10));
            registry.Bind<ISynchronizationStep, CensusQuestionnairesSynchronization>(new ConstructorArgument("sortOrder", context => 20));
            registry.Bind<ISynchronizationStep, CheckObsoleteQuestionnairesAsync>(new ConstructorArgument("sortOrder", context => 30));
            registry.Bind<ISynchronizationStep, InterviewerDownloadInterviews>(new ConstructorArgument("sortOrder", context => 40));
            registry.Bind<ISynchronizationStep, SynchronizeLogo>(new ConstructorArgument("sortOrder", context => 50));
            registry.Bind<ISynchronizationStep, SynchronizeAuditLog>(new ConstructorArgument("sortOrder", context => 60));
            registry.Bind<ISynchronizationStep, InterviewerUpdateApplication>(new ConstructorArgument("sortOrder", context => 70));
            registry.Bind<ISynchronizationStep, SynchronizeAuditLog>(new ConstructorArgument("sortOrder", context => 80));
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            return Task.CompletedTask;
        }
    }
}
