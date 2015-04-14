using System.IO;

using Android.OS;

using Cirrious.CrossCore.IoC;
using PCLStorage;
using WB.Core.BoundedContexts.QuestionnaireTester;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Files;

namespace WB.UI.QuestionnaireTester.Ninject
{
    public class NinjectIoCAdapterSetup
    {
        private const string AssembliesStoreName = "QuestionnaireAssemblies";

        public static IMvxIoCProvider CreateIocProvider()
        {
            return new NinjectMvxIocProvider(
                new SecurityModule(),
                new NetworkModule(),
                new LoggerModule(),
                new ApplicationModule(),
                new ServiceLocationModule(),
                new PlainStorageInfrastructureModule(CreatePlainStorageFolder()),
                new FileInfrastructureModule(),
                new DataCollectionModule(questionnaireAssembliesDirectoryName: GetAssembliesStorageDirectory()),
                new NinjectModuleAdapter<InfrastructureModuleMobile>(new InfrastructureModuleMobile()),
                new InterviewModule());
        }

        private static string GetAssembliesStorageDirectory()
        {
            var assembliesStorageDirectory = Directory.Exists(Environment.ExternalStorageDirectory.AbsolutePath)
                ? Environment.ExternalStorageDirectory.AbsolutePath
                : System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

            return Path.Combine(assembliesStorageDirectory, AssembliesStoreName);
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