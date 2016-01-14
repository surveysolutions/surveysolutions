using Ncqrs.Eventing.Storage;
using Ninject.Modules;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.UI.Interviewer.Implementations.Services;

namespace WB.UI.Interviewer.Ninject
{
    public class AndroidModelModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ISnapshotStore, ISnapshotStoreWithCache>().To<InMemorySnapshotStoreWithCache>().InSingletonScope();
            
            this.Bind<IPasswordHasher>().To<PasswordHasher>().InSingletonScope();
            this.Bind<IInterviewerDashboardFactory>().To<InterviewerDashboardFactory>();
        }
    }
}