using System.Linq;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ninject.Activation;
using Ninject.Extensions.Conventions;
using Ninject.Modules;
using Raven.Client.Document;
using Raven.Client.Embedded;
using RavenQuestionnaire.Core.Conventions;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Entities.Iterators;
using RavenQuestionnaire.Core.ExpressionExecutors;


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
            DocumentStoreProvider storeProvider = new DocumentStoreProvider(_repositoryPath, _isEmbeded);
            Bind<DocumentStoreProvider>().ToConstant(storeProvider);
            Bind<DocumentStore>().ToProvider<DocumentStoreProvider>().InSingletonScope();

            //Bind<IDocumentStore>().ToProvider<DocumentStoreProvider>().InSingletonScope();

            this.Kernel.Bind(x => x.FromAssembliesMatching("RavenQuestionnaire.*").SelectAllClasses().BindWith(new RegisterGenericTypesOfInterface(typeof(IViewFactory<,>))));
            this.Kernel.Bind(x => x.FromAssembliesMatching("RavenQuestionnaire.*").SelectAllClasses().BindWith(new RegisterGenericTypesOfInterface(typeof(IExpressionExecutor<,>))));
            this.Kernel.Bind(x => x.FromAssembliesMatching("RavenQuestionnaire.*").SelectAllClasses().BindWith(new RegisterGenericTypesOfInterface(typeof(Iterator<>))));
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

            this.Kernel.Bind(
                x =>
                x.FromAssembliesMatching("RavenQuestionnaire.*").SelectAllInterfaces()/*.Excluding<IFileStorageService>()*/.BindWith(new RegisterFirstInstanceOfInterface()));


        }


    }

    public class DocumentStoreProvider : Provider<DocumentStore>
    {
        public DocumentStoreProvider(string storage, bool isEmbeded)
        {
            _storage = storage;
            _isEmbeded = isEmbeded;
        }

        private readonly string _storage;
        private readonly bool _isEmbeded;
        protected override DocumentStore CreateInstance(IContext context)
        {
            DocumentStore store;
            if (_isEmbeded)
            {
                store = new EmbeddableDocumentStore()
                    {
                        DataDirectory = _storage,
                        
#if DEBUG                
                        //UseEmbeddedHttpServer = true
#endif
                    };
#if DEBUG
                //Raven.Database.Server.NonAdminHttp.EnsureCanListenToWhenInNonAdminContext(8089);
#endif
                
            }
            else
            {

                store = new DocumentStore() { Url = _storage };
            }
            store.Initialize();

            // IndexCreation.CreateIndexes(typeof(QuestionnaireContainingQuestions).Assembly, store);
            // IndexCreation.CreateIndexes(typeof(UsersInLocationIndex).Assembly, store);
            // IndexCreation.CreateIndexes(typeof(QuestionnaireGroupedByTemplateIndex).Assembly, store);
            return store;
        }
    }
}
