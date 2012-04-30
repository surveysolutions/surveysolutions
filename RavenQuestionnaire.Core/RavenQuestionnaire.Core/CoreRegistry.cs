using Ninject;
using Ninject.Activation;
using Ninject.Modules;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;
using RavenQuestionnaire.Core.Indexes;

namespace RavenQuestionnaire.Core
{
    public class CoreRegistry : NinjectModule
    {
        private string _repositoryPath;
        private bool _isWeb;

        public CoreRegistry(string repositoryPath, bool isWeb)
        {
            _repositoryPath = repositoryPath;
            _isWeb = isWeb;
        }

        public override void Load()
        {
            Bind<DocumentStoreProvider>().ToSelf().InSingletonScope().WithConstructorArgument("storage", _repositoryPath);
            Bind<IDocumentStore>().ToProvider<DocumentStoreProvider>();
            if (_isWeb)
                Bind<IDocumentSession>().ToMethod(context => Kernel.Get<IDocumentStore>().OpenSession()).InRequestScope();
            else
            {
                Bind<IDocumentSession>().ToMethod(context => Kernel.Get<IDocumentStore>().OpenSession()).InThreadScope();
            }
        }
    }

    public class DocumentStoreProvider : Provider<IDocumentStore>
    {
        public DocumentStoreProvider(string storage)
        {
            _storage = storage;
        }

        private readonly string _storage;

        protected override IDocumentStore CreateInstance(IContext context)
        {
            var store = new DocumentStore() { Url = _storage }; //System.Web.Configuration.WebConfigurationManager.AppSettings["Raven.DocumentStore"]

            store.Initialize();
            //  IndexCreation.CreateIndexes(typeof(QuestionnaireContainingQuestions).Assembly, store);
            IndexCreation.CreateIndexes(typeof(UsersInLocationIndex).Assembly, store);
            IndexCreation.CreateIndexes(typeof(QuestionnaireGroupedByTemplateIndex).Assembly, store);
            return store;
        }
    }
}
