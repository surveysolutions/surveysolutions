using Ncqrs.Eventing.Storage;
using Ninject.Modules;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services;

namespace WB.UI.QuestionnaireTester.Ninject
{
    public class ApplicationModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<DesignerApiService>().ToSelf().InSingletonScope();

            this.Bind<IEventStore>().To<InMemoryEventStore>().InSingletonScope();
            this.Bind<ISnapshotStore>().To<InMemoryEventStore>().InSingletonScope();
        }
    }
}