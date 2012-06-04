using System;
using System.Collections.Concurrent;
using System.Web;
using Ninject;
using Ninject.Activation;
using Ninject.Extensions.Conventions;
using Ninject.Modules;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;
using RavenQuestionnaire.Core.CommandHandlers.Statistics;
using RavenQuestionnaire.Core.Commands.Statistics;
using RavenQuestionnaire.Core.Indexes;

namespace RavenQuestionnaire.Core
{
    public class CoreRegistry : NinjectModule
    {
        private string _repositoryPath;
       // private bool _isWeb;

        public CoreRegistry(string repositoryPath/*, bool isWeb*/)
        {
            _repositoryPath = repositoryPath;
        //    _isWeb = isWeb;
        }

        public override void Load()
        {
            Bind<DocumentStoreProvider>().ToSelf().InSingletonScope().WithConstructorArgument("storage", _repositoryPath);
            Bind<IDocumentStore>().ToProvider<DocumentStoreProvider>().InSingletonScope();
          //  if (_isWeb)
            Bind<IDocumentSession>().ToMethod(context => GetIDocumentSession(Kernel.Get<IDocumentStore>()));
          //  Bind<ICommandHandler<GenerateQuestionnaireStatisticCommand>>().To<GenerateQuestionnaireStatisticHandler>();
            /*  kernel.Scan(s =>
            {
                s.FromAssembliesMatching("RavenQuestionnaire.*");
                s.BindWith(new GenericBindingGenerator(typeof(ICommandHandler<>)));
            });**/

            //  Bind<IDocumentSession>().ToMethod(context => Kernel.Get<IDocumentStore>().OpenSession()).When(_ => HttpContext.Current == null).InThreadScope();
            /*else
            {
                Bind<IDocumentSession>().ToMethod(context => Kernel.Get<IDocumentStore>().OpenSession()).InThreadScope();
            }*/

        }

        private IDocumentSession currentSessionRequestScope;
        private IDocumentSession currentSessionThreadScope;
        private HttpRequest request;
        private int threadId;


        private static ConcurrentDictionary<string, object> cache = new ConcurrentDictionary<string, object>();

        protected IDocumentSession GetIDocumentSession(IDocumentStore store)
        {
            var context = HttpContext.Current;
            if(context!=null)
            {
                if (context.Request != request)
                {
                    currentSessionRequestScope = new CachableDocumentSession(store, cache);
                    request = context.Request;
                }
                return currentSessionRequestScope;
            }
            var thread = System.Threading.Thread.CurrentThread;
           /* if (thread != null)
            {**/
                if (thread.ManagedThreadId != threadId)
                {
                    currentSessionThreadScope = new CachableDocumentSession(store, cache);
                    threadId = thread.ManagedThreadId;
                }
                return currentSessionThreadScope;
            /*  }
            return store.OpenSession();*/
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
