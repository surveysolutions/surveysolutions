using Ninject;
using Ninject.Modules;
using PCLStorage;
using Sqo;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Storage.Mobile.Siaqodb;

namespace WB.UI.QuestionnaireTester.Ninject
{
    public class PlainStorageInfrastructureModule : NinjectModule
    {
        public override void Load()
        {
            this.ConfigurePlainStorage();

            this.Bind<ISiaqodb>().ToConstant(new Siaqodb(CreatePlainStorageFolder()));
            
            this.Bind(typeof (IPlainStorageAccessor<>)).To(typeof (SiaqodbPlainStorageAccessor<>));
            this.Bind(typeof(IQueryablePlainStorageAccessor<>)).To(typeof(SiaqodbQueryablePlainStorageAccessor<>));
        }

        private void ConfigurePlainStorage()
        {
            this.Bind<IDocumentSerializer>().To<SiaqodbPlainStorageSerializer>().InSingletonScope();
            SiaqodbConfigurator.SetDocumentSerializer(this.Kernel.Get<IDocumentSerializer>());
        }

        private string CreatePlainStorageFolder()
        {
            var pathToPlainStorage = PortablePath.Combine(FileSystem.Current.LocalStorage.Path, "db");

            var plainStorageFolderExistingStatus = FileSystem.Current.LocalStorage.CheckExistsAsync(pathToPlainStorage).Result;

            if (plainStorageFolderExistingStatus != ExistenceCheckResult.FolderExists)
            {
                FileSystem.Current.LocalStorage.CreateFolderAsync(pathToPlainStorage, CreationCollisionOption.FailIfExists).Wait();
            }

            return pathToPlainStorage;
        }
    }
}