using Microsoft.Practices.ServiceLocation;
using PCLStorage;
using Sqo;
using WB.Core.BoundedContexts.QuestionnaireTester.Views;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services
{
    class SiaqodbFactory
    {
        static SiaqodbFactory()
        {
            CreateDbFolder();
        }

        private static async void CreateDbFolder()
        {
            pathToStorage = PortablePath.Combine(FileSystem.Current.LocalStorage.Path, "db");

            if (await FileSystem.Current.LocalStorage.CheckExistsAsync(pathToStorage) != ExistenceCheckResult.FolderExists)
            {
                await FileSystem.Current.LocalStorage.CreateFolderAsync(pathToStorage, CreationCollisionOption.FailIfExists);
            }
        }

        private static Siaqodb instance;
        private static string pathToStorage;

        public static Siaqodb GetInstance()
        {
            if (instance == null)
            {
                ConfigureDb();
                instance = new Siaqodb(pathToStorage);
            }

            return instance;
        }

        private static void ConfigureDb()
        {
            SiaqodbConfigurator.SetDocumentSerializer(ServiceLocator.Current.GetInstance<IDocumentSerializer>());
            SiaqodbConfigurator.SetDatabaseFileName<QuestionnaireMetaInfoStorageViewModel>("questionnaire-list");
            SiaqodbConfigurator.SetDatabaseFileName<QuestionnaireStorageViewModel>("questionnaires");
        }
    }

}