using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Web;
using Ninject;
using Ninject.Activation;
using Ninject.Extensions.Conventions;
using Ninject.Modules;
using Raven.Client;
using Raven.Client.Document;
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
            
            //  if (_isWeb)
            Bind<IDocumentSession>().ToMethod(
                context => new CachableDocumentSession(context.Kernel.Get<IDocumentStore>(), cache)).InScope(
                    x => HttpContext.Current.Request);
            Bind<IClientSettingsProvider>().To<RavenQuestionnaire.Core.ClientSettingsProvider.ClientSettingsProvider>().
                InSingletonScope();
         /*   this.Kernel.Bind(x => x
                                      .FromAssembliesMatching("RavenQuestionnaire.*")
                                      .SelectAllClasses().Excluding<RavenQuestionnaire.Core.ClientSettingsProvider.ClientSettingsProvider>()
                                      .BindDefaultInterface());*/
           // Bind<IViewFactory<CQGroupedBrowseInputModel, CQGroupedBrowseView>>().To<CQGroupedBrowseFactory>();
           /* Bind(typeof(ILazy<>)).ToMethod(ctx =>
            {
                var targetType = typeof(LazyLoader<>).MakeGenericType(ctx.GenericArguments);
                return ctx.Kernel.Get(targetType);
            });*/
            this.Kernel.Bind(x => x.FromAssembliesMatching("RavenQuestionnaire.*").SelectAllClasses().BindWith(new RegisterGenericTypesOfInterface(typeof(IViewFactory<,>))));
            this.Kernel.Bind(x => x.FromAssembliesMatching("RavenQuestionnaire.*").SelectAllClasses().BindWith(new RegisterGenericTypesOfInterface(typeof(ICommandHandler<>))));
             
             this.Kernel.Bind(x => x.FromAssembliesMatching("RavenQuestionnaire.*").SelectAllInterfaces().BindWith(new RegisterGenericTypesOfInterface(typeof(IExpressionExecutor<,>))));
             this.Kernel.Bind(x => x.FromAssembliesMatching("RavenQuestionnaire.*").SelectAllInterfaces().BindWith(new RegisterGenericTypesOfInterface(typeof(Iterator<>))));
             this.Kernel.Bind(x => x.FromAssembliesMatching("RavenQuestionnaire.*").SelectAllInterfaces().BindWith(new RegisterGenericTypesOfInterface(typeof(IEntitySubscriber<>))));
             this.Kernel.Bind(x => x.FromAssembliesMatching("RavenQuestionnaire.*").SelectAllInterfaces().Excluding<IClientSettingsProvider>().BindWith(new RegisterFirstInstanceOfInterface()));
            /*   this.Kernel.Scan(s =>
                                 {
                                     s.FromAssembliesMatching("RavenQuestionnaire.*");
                                     s.BindWith(new GenericBindingGenerator(typeof (ICommandHandler<>)));
                                 });

            this.Kernel.Scan(s =>
                                 {
                                     s.FromAssembliesMatching("RavenQuestionnaire.*");
                                     s.BindWith(new GenericBindingGenerator(typeof (IViewFactory<,>)));
                                 });
            this.Kernel.Scan(s =>
                                 {
                                     s.FromAssembliesMatching("RavenQuestionnaire.*");
                                     s.BindWith(new GenericBindingGenerator(typeof (IExpressionExecutor<,>)));
                                 });*/
            //new GenericBindingGenerator();
            //Bind<ICommandHandler<>>().To<COm>()

            /* this.Kernel.Scan(s =>
                                 {
                                     s.FromAssembliesMatching("RavenQuestionnaire.*");
                                     s.BindWith(new RegisterFirstInstanceOfInterface());
                                 });*/
            /* this.Kernel.Scan(s =>
                                 {
                                     s.FromAssembliesMatching("RavenQuestionnaire.*");
                                     s.BindWith(new GenericBindingGenerator(typeof (Iterator<>)));
                                 });
            this.Kernel.Scan(s =>
                                 {
                                     s.FromAssembliesMatching("RavenQuestionnaire.*");
                                     s.BindWith(new GenericBindingGenerator(typeof (IEntitySubscriber<>)));
                                 });*/

        }
        private static ConcurrentDictionary<string, object> cache = new ConcurrentDictionary<string, object>();
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
            var store = new DocumentStore() { Url = _storage }; //System.Web.Configuration.WebConfigurationManager.AppSettings["Raven.DocumentStore"]
            
            store.Initialize();
            
            //  IndexCreation.CreateIndexes(typeof(QuestionnaireContainingQuestions).Assembly, store);
            IndexCreation.CreateIndexes(typeof(UsersInLocationIndex).Assembly, store);
            IndexCreation.CreateIndexes(typeof(QuestionnaireGroupedByTemplateIndex).Assembly, store);
            return store;
        }
    }
}
