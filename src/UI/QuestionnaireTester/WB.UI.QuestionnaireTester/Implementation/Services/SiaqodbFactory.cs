using Microsoft.Practices.ServiceLocation;
using PCLStorage;
using Sqo;

namespace WB.UI.QuestionnaireTester.Implementation.Services
{
    class SiaqodbFactory
    {
        private static Siaqodb instance;
        public static Siaqodb GetInstance()
        {
            if (instance == null)
            {
                SiaqodbConfigurator.SetDocumentSerializer(ServiceLocator.Current.GetInstance<IDocumentSerializer>());
                instance = new Siaqodb(FileSystem.Current.LocalStorage.Path);
            }

            return instance;
        }
    }

}