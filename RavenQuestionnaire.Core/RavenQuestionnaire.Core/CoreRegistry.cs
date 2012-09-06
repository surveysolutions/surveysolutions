// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CoreRegistry.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The core registry.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core
{
    using System;
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

    /// <summary>
    /// The core registry.
    /// </summary>
    public class CoreRegistry : NinjectModule
    {
        #region Fields

        /// <summary>
        /// The _is embeded.
        /// </summary>
        private readonly bool isEmbeded;

        /// <summary>
        /// The _repository path.
        /// </summary>
        private readonly string repositoryPath;

        #endregion

        // private bool _isWeb;
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreRegistry"/> class.
        /// </summary>
        /// <param name="repositoryPath">
        /// The repository path.
        /// </param>
        /// <param name="isEmbeded">
        /// The is embeded.
        /// </param>
        public CoreRegistry(string repositoryPath, bool isEmbeded)
        {
            this.repositoryPath = repositoryPath;
            this.isEmbeded = isEmbeded;

            // _isWeb = isWeb;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The load.
        /// </summary>
        public override void Load()
        {
            var storeProvider = new DocumentStoreProvider(this.repositoryPath, this.isEmbeded);
            this.Bind<DocumentStoreProvider>().ToConstant(storeProvider);
            this.Bind<DocumentStore>().ToProvider<DocumentStoreProvider>().InSingletonScope();

            // Bind<IDocumentStore>().ToProvider<DocumentStoreProvider>().InSingletonScope();
            this.Kernel.Bind(
                x =>
                x.FromAssembliesMatching("RavenQuestionnaire.*").SelectAllClasses().BindWith(
                    new RegisterGenericTypesOfInterface(typeof(IViewFactory<,>))));
            this.Kernel.Bind(
                x =>
                x.FromAssembliesMatching("RavenQuestionnaire.*").SelectAllClasses().BindWith(
                    new RegisterGenericTypesOfInterface(typeof(IExpressionExecutor<,>))));
            this.Kernel.Bind(
                x =>
                x.FromAssembliesMatching("RavenQuestionnaire.*").SelectAllClasses().BindWith(
                    new RegisterGenericTypesOfInterface(typeof(Iterator<>))));
            this.Kernel.Bind(
                scanner =>
                scanner.FromAssembliesMatching("RavenQuestionnaire.*").Select(
                    t =>
                    t.GetInterfaces().FirstOrDefault(
                        i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDenormalizerStorage<>)) != null)
                    .BindAllInterfaces().Configure(binding => binding.InSingletonScope()));

            this.Kernel.Bind(
                scanner =>
                scanner.FromAssembliesMatching("RavenQuestionnaire.*").Select(
                    t =>
                    t.GetInterfaces().FirstOrDefault(
                        i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>)) != null).
                    BindAllInterfaces());

            this.Kernel.Bind(
                x => x.FromAssembliesMatching("RavenQuestionnaire.*").SelectAllInterfaces()
                    
                         /*.Excluding<IFileStorageService>()*/.BindWith(new RegisterFirstInstanceOfInterface()));
        }

        #endregion
    }

    /// <summary>
    /// The document store provider.
    /// </summary>
    public class DocumentStoreProvider : Provider<DocumentStore>
    {
        #region Fields

        /// <summary>
        /// The _is embeded.
        /// </summary>
        private readonly bool isEmbeded;

        /// <summary>
        /// The _storage.
        /// </summary>
        private readonly string storage;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentStoreProvider"/> class.
        /// </summary>
        /// <param name="storage">
        /// The storage.
        /// </param>
        /// <param name="isEmbeded">
        /// The is embeded.
        /// </param>
        public DocumentStoreProvider(string storage, bool isEmbeded)
        {
            this.storage = storage;
            this.isEmbeded = isEmbeded;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The create instance.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// The Raven.Client.Document.DocumentStore.
        /// </returns>
        protected override DocumentStore CreateInstance(IContext context)
        {
            DocumentStore store = null;
            try
            {
                if (this.isEmbeded)
                {
                    store = new EmbeddableDocumentStore
                    {
                        DataDirectory = this.storage
                    };
                }
                else
                {
                    store = new DocumentStore { Url = this.storage };
                }

                store.Initialize();
            }
            catch (Exception ex)
            {
                throw;// new Exception(ex.Message, ex);
            }

            return store;
        }

        #endregion
    }
}