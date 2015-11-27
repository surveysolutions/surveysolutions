using Ncqrs.Eventing.Storage;
using Ninject.Modules;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.Infrastructure.Implementation.Storage;
using WB.UI.Interviewer.FileStorage;

namespace WB.UI.Interviewer.Ninject
{
    public class AndroidModelModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ISnapshotStore>().To<InMemoryCachedSnapshotStore>().InSingletonScope();

            this.Bind<IFileStorageService>().To<FileStorageService>().InSingletonScope();
            this.Bind<IPasswordHasher>().To<PasswordHasher>().InSingletonScope();
            this.Bind<IInterviewerDashboardFactory>().To<InterviewerDashboardFactory>();
        }
    }
}