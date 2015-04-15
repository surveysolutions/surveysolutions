using Cirrious.CrossCore.IoC;
using PCLStorage;
using WB.Core.BoundedContexts.QuestionnaireTester;
using WB.Core.Infrastructure;

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
                new PlainStorageInfrastructureModule(GetPathToSubfolderInLocalDirectory("database")),
                new DataCollectionModule(pathToQuestionnaireAssemblies: GetPathToSubfolderInLocalDirectory("libraries")),
                new NinjectModuleAdapter<InfrastructureModuleMobile>(new InfrastructureModuleMobile()),
                new InterviewModule());
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