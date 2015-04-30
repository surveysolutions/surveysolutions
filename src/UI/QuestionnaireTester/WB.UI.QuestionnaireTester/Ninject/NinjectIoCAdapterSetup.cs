using Cirrious.CrossCore.IoC;
using PCLStorage;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Android;

namespace WB.UI.QuestionnaireTester.Ninject
{
    public class NinjectIoCAdapterSetup
    {
        public static IMvxIoCProvider CreateIocProvider()
        {
            return new NinjectMvxIocProvider(
                new AndroidInfrastructureModule(pathToQuestionnaireAssemblies: GetPathToSubfolderInLocalDirectory("libraries"),
                    plainStorageSettings: new PlainStorageSettings(){ StorageFolderPath = GetPathToSubfolderInLocalDirectory("database") }),
                new ApplicationModule(),
                new PlainStorageInfrastructureModule(),
                new DataCollectionModule(),
                new NinjectModuleAdapter<InfrastructureModuleMobile>(new InfrastructureModuleMobile()));
        }

        private static string GetPathToSubfolderInLocalDirectory(string subFolderName)
        {
            var pathToSubfolderInLocalDirectory = PortablePath.Combine(FileSystem.Current.LocalStorage.Path, subFolderName);

            var subfolderExistingStatus = FileSystem.Current.LocalStorage.CheckExistsAsync(pathToSubfolderInLocalDirectory).Result;
            if (subfolderExistingStatus != ExistenceCheckResult.FolderExists)
            {
                FileSystem.Current.LocalStorage.CreateFolderAsync(pathToSubfolderInLocalDirectory, CreationCollisionOption.FailIfExists).Wait();
            }

            return pathToSubfolderInLocalDirectory;
        }
    }
}