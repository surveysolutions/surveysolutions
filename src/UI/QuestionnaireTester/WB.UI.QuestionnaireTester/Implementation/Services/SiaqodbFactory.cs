using Microsoft.Practices.ServiceLocation;
using PCLStorage;
using Sqo;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.UI.QuestionnaireTester.Implementation.Services
{
    class SiaqodbFactory
    {
        static SiaqodbFactory()
        {
            CreateDbFolder();
        }

        private static async void CreateDbFolder()
        {
            pathToPersonalUserStorage = PortablePath.Combine(FileSystem.Current.LocalStorage.Path,
                ServiceLocator.Current.GetInstance<IPrincipal>().CurrentIdentity.Name);

            if (await FileSystem.Current.LocalStorage.CheckExistsAsync(pathToPersonalUserStorage) != ExistenceCheckResult.FolderExists)
            {
                await FileSystem.Current.LocalStorage.CreateFolderAsync(pathToPersonalUserStorage, CreationCollisionOption.FailIfExists);
            }
        }

        private static Siaqodb instance;
        private static string pathToPersonalUserStorage;

        public static Siaqodb GetInstance()
        {
            if (instance == null)
            {
                SiaqodbConfigurator.SetDocumentSerializer(ServiceLocator.Current.GetInstance<IDocumentSerializer>());
                instance = new Siaqodb(pathToPersonalUserStorage);
            }

            return instance;
        }
    }

}