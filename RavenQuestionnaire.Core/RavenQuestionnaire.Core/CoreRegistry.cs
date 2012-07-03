using System.Linq;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ninject.Activation;
using Ninject.Extensions.Conventions;
using Ninject.Modules;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
using Raven.Client.Indexes;
using RavenQuestionnaire.Core.ClientSettingsProvider;
using RavenQuestionnaire.Core.Conventions;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Entities.Iterators;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Indexes;

namespace RavenQuestionnaire.Core
{
    public class CoreRegistry : NinjectModule
    {
        private string _repositoryPath;
        private bool _isEmbeded;
        // private bool _isWeb;

        public CoreRegistry(string repositoryPath, bool isEmbeded)
        {
            _repositoryPath = repositoryPath;
            _isEmbeded = isEmbeded;
            //    _isWeb = isWeb;
        }

        public override void Load()
        {
            DocumentStoreProvider storeProvider=new DocumentStoreProvider(_repositoryPath,_isEmbeded);
            Bind<DocumentStoreProvider>().ToConstant(storeProvider);
            Bind<IDocumentStore>().ToProvider<DocumentStoreProvider>().InSingletonScope();

       /*     Bind<IDocumentSession>().ToMethod(
                context => new CachableDocumentSession(context.Kernel.Get<IDocumentStore>(), cache)).When(
                    b => HttpContext.Current != null).InScope(o => HttpContext.Current);

            Bind<IDocumentSession>().ToMethod(
                context => new CachableDocumentSession(context.Kernel.Get<IDocumentStore>(), cache)).When(
                    b => HttpContext.Current == null).InScope(o => Thread.CurrentThread);*/

      /*      Bind<IDocumentSession>().ToMethod(
                context => new CachableDocumentSession(context.Kernel.Get<IDocumentStore>(), cache)).When(
                    request => request.ParentRequest.ParentRequest.Service == typeof (ICommandHandler<>));*/
            Bind<IClientSettingsProvider>().To<RavenQuestionnaire.Core.ClientSettingsProvider.ClientSettingsProvider>().
                InSingletonScope();
           // this.Kernel.BindInterfaceToBinding<ICommandInvoker, IDocumentSession>();
            
            this.Kernel.Bind(x => x.FromAssembliesMatching("RavenQuestionnaire.*").SelectAllClasses().BindWith(new RegisterGenericTypesOfInterface(typeof(IViewFactory<,>))));
            this.Kernel.Bind(x => x.FromAssembliesMatching("RavenQuestionnaire.*").SelectAllClasses().BindWith(new RegisterGenericTypesOfInterface(typeof(ICommandHandler<>))));

            this.Kernel.Bind(x => x.FromAssembliesMatching("RavenQuestionnaire.*").SelectAllClasses().BindWith(new RegisterGenericTypesOfInterface(typeof(IExpressionExecutor<,>))));
            this.Kernel.Bind(x => x.FromAssembliesMatching("RavenQuestionnaire.*").SelectAllClasses().BindWith(new RegisterGenericTypesOfInterface(typeof(Iterator<>))));
            this.Kernel.Bind(x => x.FromAssembliesMatching("RavenQuestionnaire.*").SelectAllClasses().BindWith(new RegisterGenericTypesOfInterface(typeof(IEventSubscriber<>))));
            //this.Kernel.Bind(x => x.FromAssembliesMatching("RavenQuestionnaire.*").SelectAllClasses().BindWith(new RegisterGenericTypesOfInterface(typeof(IEntitySubscriber<>))));
       
            this.Kernel.Bind(scanner => scanner.FromAssembliesMatching("RavenQuestionnaire.*")
                                            .Select(
                                                t =>
                                                t.GetInterfaces().FirstOrDefault(
                                                    i =>
                                                    i.IsGenericType &&
                                                    i.GetGenericTypeDefinition() == typeof(IDenormalizerStorage<>)) !=
                                                null).BindAllInterfaces()
                                            .Configure(binding => binding.InSingletonScope()));


            this.Kernel.Bind(scanner => scanner.FromAssembliesMatching("RavenQuestionnaire.*")
                                            .Select(
                                                t =>
                                                t.GetInterfaces().FirstOrDefault(
                                                    i =>
                                                    i.IsGenericType &&
                                                    i.GetGenericTypeDefinition() == typeof(IEventHandler<>)) !=
                                                null).BindAllInterfaces());
           
          /*  this.Kernel.Bind(scanner => scanner.FromAssembliesMatching("RavenQuestionnaire.*")
                                           .Select(
                                               t =>
                                               t.GetInterfaces().FirstOrDefault(
                                                   i =>
                                                   i.IsGenericType &&
                                                   i.GetGenericTypeDefinition() == typeof(IEventHandler<>)) !=
                                               null).BindAllInterfaces()
                                           .Configure(binding => binding.InSingletonScope()));*/
  
            this.Kernel.Bind(
                x =>
                x.FromAssembliesMatching("RavenQuestionnaire.*").SelectAllInterfaces().Excluding
                    <IClientSettingsProvider>().BindWith(new RegisterFirstInstanceOfInterface()));


        }
       
   /*     private ConcurrentDictionary<HttpRequest, IDocumentSession> currentSessionRequestScope;
        private  ConcurrentDictionary<int, IDocumentSession> currentSessionThreadScope;
   


      

        protected IDocumentSession GetIDocumentSession(IDocumentStore store)
        {
            IDocumentSession session;
            var context = HttpContext.Current;
            if (context != null)
            {
                try
                {
                    if (!currentSessionRequestScope.ContainsKey(context.Request))
                    {
                        session = new CachableDocumentSession(store, cache);
                        
                        currentSessionRequestScope.TryAdd(context.Request, session);
                    }
                    return currentSessionRequestScope[context.Request];
                }
                catch (HttpException)
                {

                }
            }
            var thread = System.Threading.Thread.CurrentThread;
          
            if (currentSessionThreadScope.ContainsKey(thread.ManagedThreadId))
            {
                session = new CachableDocumentSession(store, cache);
                currentSessionThreadScope.TryAdd(thread.ManagedThreadId, session);
            }
            return currentSessionThreadScope[thread.ManagedThreadId];
        }*/
    }

    public class DocumentStoreProvider : Provider<IDocumentStore>
    {
        public DocumentStoreProvider(string storage, bool isEmbeded)
        {
            _storage = storage;
            _isEmbeded = isEmbeded;
        }

        private readonly string _storage;
        private readonly bool _isEmbeded;
        protected override IDocumentStore CreateInstance(IContext context)
        {
            IDocumentStore store;
            if (_isEmbeded)
            {

                store = new EmbeddableDocumentStore() {DataDirectory = _storage, UseEmbeddedHttpServer = true};
                    //System.Web.Configuration.WebConfigurationManager.AppSettings["Raven.DocumentStore"]
                Raven.Database.Server.NonAdminHttp.EnsureCanListenToWhenInNonAdminContext(8080);
            }
            else
            {
                
                store = new DocumentStore() {Url = _storage};
            }
            store.Initialize();
            
            //  IndexCreation.CreateIndexes(typeof(QuestionnaireContainingQuestions).Assembly, store);
            IndexCreation.CreateIndexes(typeof(CompleteQuestionnaireByStatus).Assembly, store);
            IndexCreation.CreateIndexes(typeof(UsersInLocationIndex).Assembly, store);
            IndexCreation.CreateIndexes(typeof(QuestionnaireGroupedByTemplateIndex).Assembly, store);
            return store;
        }
    }
}
