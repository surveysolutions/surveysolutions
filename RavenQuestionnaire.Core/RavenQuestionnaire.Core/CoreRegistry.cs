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
    using System.Linq;

    using Main.Core.Entities.Iterators;
    using Main.Core.ExpressionExecutors;

    using Ncqrs.Eventing.ServiceModel.Bus;

    using Ninject.Extensions.Conventions;
    using Ninject.Modules;

    using Raven.Client.Document;

    using Main.Core.Conventions;
    using RavenQuestionnaire.Core.Denormalizers;

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
                x.FromAssembliesMatching("Main.Core").SelectAllClasses().BindWith(
                    new RegisterGenericTypesOfInterface(typeof(IExpressionExecutor<,>))));
            this.Kernel.Bind(
                x =>
                x.FromAssembliesMatching("Main.Core").SelectAllClasses().BindWith(
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
                scanner.FromAssembliesMatching("Main.Core").Select(
                    t =>
                    t.GetInterfaces().FirstOrDefault(
                        i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>)) != null).
                    BindAllInterfaces());

            this.Kernel.Bind(
                scanner =>
                scanner.FromAssembliesMatching("RavenQuestionnaire.*").Select(
                    t =>
                    t.GetInterfaces().FirstOrDefault(
                        i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>)) != null).
                    BindAllInterfaces());


            this.Kernel.Bind(
                x => x.FromAssembliesMatching("Main.Core").SelectAllInterfaces()
                    /*.Excluding<IFileStorageService>()*/.BindWith(new RegisterFirstInstanceOfInterface()));

            this.Kernel.Bind(
                x => x.FromAssembliesMatching("RavenQuestionnaire.*").SelectAllInterfaces()
                         /*.Excluding<IFileStorageService>()*/.BindWith(new RegisterFirstInstanceOfInterface()));
        }

        #endregion
    }
}