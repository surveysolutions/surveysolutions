using System;
using System.IO;
using Cirrious.CrossCore.IoC;
using PCLStorage;
using WB.Core.BoundedContexts.Tester;
using WB.Core.BoundedContexts.Tester.Infrastructure;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Android;
using WB.Core.Infrastructure.Ncqrs;
using WB.Infrastructure.Shared.Enumerator.Ninject;

namespace WB.UI.Tester.Ninject
{
    public class NinjectIoCAdapterSetup
    {
        public static IMvxIoCProvider CreateIocProvider()
        {
            var basePath = Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Personal))
               ? Environment.GetFolderPath(Environment.SpecialFolder.Personal)
               : Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;

            return new NinjectMvxIocProvider(
                new AndroidInfrastructureModule(pathToQuestionnaireAssemblies: GetPathToSubfolderInLocalDirectory("libraries"),
                    plainStorageSettings: new PlainStorageSettings(){ StorageFolderPath = GetPathToSubfolderInLocalDirectory("database") }),
                new ApplicationModule(),
                new PlainStorageInfrastructureModule(),
                new EnumeratorSharedKernelModule(),
                new TesterBoundedContextModule(),
                new DataCollectionModule(),
                new EnumeratorInfrastructureModule(basePath),
                new NcqrsModule().AsNinject(),
                new InfrastructureModuleMobile().AsNinject());
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