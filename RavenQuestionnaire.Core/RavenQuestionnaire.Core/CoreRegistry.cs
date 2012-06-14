using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Web;
using Ninject;
using Ninject.Activation;
using Ninject.Extensions.Conventions;
using Ninject.Extensions.NamedScope;
using Ninject.Extensions.ContextPreservation;
using Ninject.Modules;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
using Raven.Client.Indexes;
using RavenQuestionnaire.Core.ClientSettingsProvider;
using RavenQuestionnaire.Core.CommandHandlers.Statistics;
using RavenQuestionnaire.Core.Commands.Statistics;
using RavenQuestionnaire.Core.Conventions;
using RavenQuestionnaire.Core.Entities.Iterators;
using RavenQuestionnaire.Core.Entities.Subscribers;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Indexes;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Grouped;

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
            this.Kernel.Bind(x => x.FromAssembliesMatching("RavenQuestionnaire.*").SelectAllClasses().BindWith(new RegisterGenericTypesOfInterface(typeof(IEntitySubscriber<>))));
             this.Kernel.Bind(x => x.FromAssembliesMatching("RavenQuestionnaire.*").SelectAllInterfaces().Excluding<IClientSettingsProvider>().BindWith(new RegisterFirstInstanceOfInterface()));
            

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
        public DocumentStoreProvider(string storage)
        {
            _storage = storage;
        }

        private readonly string _storage;

        protected override IDocumentStore CreateInstance(IContext context)
        {
            var store = new EmbeddableDocumentStore() { DataDirectory = _storage, UseEmbeddedHttpServer = true }; //System.Web.Configuration.WebConfigurationManager.AppSettings["Raven.DocumentStore"]
            Raven.Database.Server.NonAdminHttp.EnsureCanListenToWhenInNonAdminContext(8080);
            store.Initialize();
            
            //  IndexCreation.CreateIndexes(typeof(QuestionnaireContainingQuestions).Assembly, store);
            IndexCreation.CreateIndexes(typeof(UsersInLocationIndex).Assembly, store);
            IndexCreation.CreateIndexes(typeof(QuestionnaireGroupedByTemplateIndex).Assembly, store);
            return store;
        }
    }
}
