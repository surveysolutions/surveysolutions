// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NCQRSInit.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The ncqrs init.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Commands;
    using Main.Core.Services;

    using Ncqrs;
    using Ncqrs.Commanding.CommandExecution.Mapping;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
    using Ncqrs.Commanding.ServiceModel;
    using Ncqrs.Eventing.ServiceModel.Bus;
    using Ncqrs.Eventing.Sourcing.Snapshotting;
    using Ncqrs.Eventing.Storage;
    using Ncqrs.Eventing.Storage.RavenDB;

    using Ninject;

    using Raven.Client.Document;

    /// <summary>
    /// The ncqrs init.
    /// </summary>
    public static class NCQRSInit
    {
        #region Public Methods and Operators

        /// <summary>
        /// The init.
        /// </summary>
        /// <param name="kernel">
        /// The kernel.
        /// </param>
        public static void Init(IKernel kernel)
        {
            var store = InitializeEventStore(kernel.Get<DocumentStore>());

            NcqrsEnvironment.SetDefault<IStreamableEventStore>(store);
            NcqrsEnvironment.SetDefault<IEventStore>(store); // usage in framework 

            NcqrsEnvironment.SetDefault(InitializeCommandService(kernel.Get<ICommandListSupplier>()));

            NcqrsEnvironment.SetDefault<ISnapshottingPolicy>(new SimpleSnapshottingPolicy(1));

            // key param for storing im memory
            NcqrsEnvironment.SetDefault<ISnapshotStore>(new InMemoryEventStore());
            NcqrsEnvironment.SetDefault(kernel.Get<IFileStorageService>());
            var bus = new InProcessEventBus(true);
            RegisterEventHandlers(bus, kernel);

            NcqrsEnvironment.SetDefault<IEventBus>(bus);
        }

        /// <summary>
        /// The rebuild read layer.
        /// </summary>
        /// <param name="kernel">
        /// The kernel.
        /// </param>
        /// <exception cref="Exception">
        /// </exception>
        public static void RebuildReadLayer(IKernel kernel)
        {
            var myEventBus = NcqrsEnvironment.Get<IEventBus>();
            if (myEventBus == null)
            {
                throw new Exception("IEventBus is not properly initialized.");
            }

            var myEventStore = NcqrsEnvironment.Get<IStreamableEventStore>(); 

            if (myEventStore == null)
            {
                throw new Exception("IStreamableEventStore is not correctly initialized.");
            }

            // store.CreateIndex();
            // var myEvents = store.GetAllEvents();
            myEventBus.Publish(myEventStore.GetEventStream());
        }

        #endregion

        #region Methods

        /// <summary>
        /// The initialize command service.
        /// </summary>
        /// <param name="commandSupplier">
        /// The command Supplier.
        /// </param>
        /// <returns>
        /// The Ncqrs.Commanding.ServiceModel.ICommandService.
        /// </returns>
        private static ICommandService InitializeCommandService(ICommandListSupplier commandSupplier)
        {
            var mapper = new AttributeBasedCommandMapper();
            var service = new ConcurrencyResolveCommandService();

            foreach (Type type in commandSupplier.GetCommandList())
            {
                service.RegisterExecutor(type, new UoWMappedCommandExecutor(mapper));
            }

            return service;
        }

        /// <summary>
        /// The initialize event store.
        /// </summary>
        /// <param name="store">
        /// The store.
        /// </param>
        /// <returns>
        /// The Ncqrs.Eventing.Storage.IEventStore.
        /// </returns>
        private static IStreamableEventStore InitializeEventStore(DocumentStore store)
        {
            return new RavenDBEventStore(store);
        }

        /// <summary>
        /// The is i event handler interface.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        private static bool IsIEventHandlerInterface(Type type)
        {
            return type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEventHandler<>);
        }

        /// <summary>
        /// The register event handlers.
        /// </summary>
        /// <param name="bus">
        /// The bus.
        /// </param>
        /// <param name="kernel">
        /// The kernel.
        /// </param>
        private static void RegisterEventHandlers(InProcessEventBus bus, IKernel kernel)
        {
            IEnumerable<object> handlers = kernel.GetAll(typeof(IEventHandler<>)).Distinct();
            foreach (object handler in handlers)
            {
                IEnumerable<Type> ieventHandlers = handler.GetType().GetInterfaces().Where(IsIEventHandlerInterface);
                foreach (Type ieventHandler in ieventHandlers)
                {
                    bus.RegisterHandler(handler, ieventHandler.GetGenericArguments()[0]);
                }
            }
        }

        #endregion
    }
}