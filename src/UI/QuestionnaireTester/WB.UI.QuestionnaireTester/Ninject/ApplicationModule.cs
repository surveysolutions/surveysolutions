using Ncqrs.Eventing.Storage;
using Ninject.Modules;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services;
using WB.Core.GenericSubdomains.Utils.Rest;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.UI.QuestionnaireTester.Implementation.Services;

namespace WB.UI.QuestionnaireTester.Ninject
{
    public class ApplicationModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<JsonUtilsSettings>().ToSelf().InSingletonScope();
            this.Bind<IJsonUtils>().To<NewtonJsonUtils>().InSingletonScope();

            this.Bind<ApplicationSettings>().ToSelf().InSingletonScope();
            this.Bind<DesignerApiService>().ToSelf().InSingletonScope();

            var evenStore = new InMemoryEventStore();
            var snapshotStore = new InMemoryEventStore();


            this.Bind<IEventStore>().ToConstant(evenStore);
            this.Bind<ISnapshotStore>().ToConstant(snapshotStore);
        }
    }
}