using Cirrious.CrossCore.IoC;
using PCLStorage;
using WB.Core.Infrastructure;
using WB.Core.SharedKernels.DataCollection;

namespace WB.UI.QuestionnaireTester.Ninject
{
    public class NinjectIoCAdapterSetup
    {
        public static IMvxIoCProvider CreateIocProvider()
        {
            return new NinjectMvxIocProvider(
                new SecurityModule(),
                new NetworkModule(),
                new LoggerModule(),
                new ApplicationModule(),
                new ServiceLocationModule(),
                new PlainStorageInfrastructureModule(CreatePlainStorageFolder()),
                new DataCollectionModule(localFileStoragePath: FileSystem.Current.LocalStorage.Path, questionnaireAssembliesDirectoryName: "assemblies"),
                new NinjectModuleAdapter<InfrastructureModuleMobile>(new InfrastructureModuleMobile()));
        }

        private static string CreatePlainStorageFolder()
        {
            var pathToPlainStorage = PortablePath.Combine(FileSystem.Current.LocalStorage.Path, "database");

            var plainStorageFolderExistingStatus = FileSystem.Current.LocalStorage.CheckExistsAsync(pathToPlainStorage).Result;
            if (plainStorageFolderExistingStatus != ExistenceCheckResult.FolderExists)
            {
                FileSystem.Current.LocalStorage.CreateFolderAsync(pathToPlainStorage, CreationCollisionOption.FailIfExists).Wait();
            }

            return pathToPlainStorage;
        }
    }
}