using Ncqrs.Eventing.Storage;
using Ninject.Modules;

using WB.Core.BoundedContexts.Tester.Services;
using WB.UI.Tester.CustomServices.UserInteraction;

namespace WB.UI.Tester.Ninject
{
    public class ApplicationModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IUserInteractionService>().To<UserInteractionService>();

            this.Bind<IEventStore>().To<InMemoryEventStore>().InSingletonScope();
            this.Bind<ISnapshotStore>().To<InMemoryEventStore>().InSingletonScope();
        }
    }
}