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
        public CoreRegistry()
        {
        }

        public override void Load()
        {
            Bind<DocumentStoreProvider>().ToSelf().InSingletonScope();
            Bind<IDocumentStore>().ToProvider<DocumentStoreProvider>();
            Bind<IDocumentSession>().ToMethod(context => Kernel.Get<IDocumentStore>().OpenSession()).InRequestScope();
        }
    }

    public class DocumentStoreProvider : Provider<IDocumentStore>
    {
        protected override IDocumentStore CreateInstance(IContext context)
        {
            var store = new DocumentStore()
                            {Url = System.Web.Configuration.WebConfigurationManager.AppSettings["Raven.DocumentStore"]};

            store.Initialize();
          //  IndexCreation.CreateIndexes(typeof(QuestionnaireContainingQuestions).Assembly, store);
            IndexCreation.CreateIndexes(typeof(UsersInLocationIndex).Assembly, store);
            return store;
        }
    }
}
