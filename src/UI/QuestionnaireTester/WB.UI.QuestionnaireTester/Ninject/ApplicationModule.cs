using Ncqrs.Eventing.Storage;
using Ninject.Modules;
using WB.Core.GenericSubdomains.Utils.Rest;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.UI.QuestionnaireTester.Implementation.Services;

namespace WB.UI.QuestionnaireTester.Ninject
{
    public class ApplicationModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IFileSystemAccessor>().To<FileSystemAccessor>().InSingletonScope();

            this.Bind<JsonUtilsSettings>().ToConstant(new JsonUtilsSettings() { TypeNameHandling = TypeSerializationSettings.None });
            this.Bind<IJsonUtils>().To<NewtonJsonUtils>().InSingletonScope();
            this.Bind<ApplicationSettings>().ToSelf().InSingletonScope();

            var evenStore = new InMemoryEventStore();
            var snapshotStore = new InMemoryEventStore();


            this.Bind<IEventStore>().ToConstant(evenStore);
            this.Bind<ISnapshotStore>().ToConstant(snapshotStore);
        }
    }
}