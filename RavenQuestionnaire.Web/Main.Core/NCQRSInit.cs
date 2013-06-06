// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NCQRSInit.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The ncqrs init.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using WB.Core.SharedKernel.Utils.Logging;

namespace Main.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Commands;
    using Main.Core.Services;

    using Ncqrs;
    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
    using Ncqrs.Commanding.ServiceModel;
    using Ncqrs.Eventing;
    using Ncqrs.Eventing.ServiceModel.Bus;
    using Ncqrs.Eventing.Sourcing.Snapshotting;
    using Ncqrs.Eventing.Storage;

    using Main.Core.Entities.Extensions;
    using Ncqrs.Domain.Storage;
    using Ncqrs.Restoring.EventStapshoot;
    using Ncqrs.Restoring.EventStapshoot.EventStores;

#if MONODROID
    using AndroidNcqrs.Eventing.Storage.SQLite;
#else
    using Ncqrs.Eventing.Storage.RavenDB;
    using Raven.Client.Document;
#endif


    //using Ncqrs.Eventing.Storage.RavenDB;

    using Ninject;

    /*using Raven.Client.Document;*/

    /// <summary>
    /// The ncqrs init.
    /// </summary>
    public static class NcqrsInit
    {

        private static bool isReadLayerBuilt = false;
        private static object lockObject = new object();

        public static bool IsReadLayerBuilt
        {
            get { return isReadLayerBuilt; }
        }

        #region Public Methods and Operators

        public static void Init(IKernel kernel)
        {
            Init(kernel, 50);
        }

        public static void Init(IKernel kernel, int pageSize)
        {
#if MONODROID
            NcqrsEnvironment.SetDefault(kernel.Get<IEventStore>());
            //NcqrsEnvironment.SetDefault<IStreamableEventStore>(kernel.Get<IStreamableEventStore>());
#else
            
            var store = InitializeEventStore(kernel.Get<DocumentStore>(), pageSize);
            NcqrsEnvironment.SetDefault<IStreamableEventStore>(store);
            NcqrsEnvironment.SetDefault<IEventStore>(store); // usage in framework 
            kernel.Bind<IStreamableEventStore>().ToConstant(store);

            NcqrsEnvironment.SetDefault(InitializeCommandService(kernel.Get<ICommandListSupplier>()));

            NcqrsEnvironment.SetDefault(kernel.Get<IFileStorageService>());
#endif

            NcqrsEnvironment.SetDefault(InitializeCommandService(kernel.Get<ICommandListSupplier>()));

            NcqrsEnvironment.SetDefault<ISnapshottingPolicy>(new SimpleSnapshottingPolicy(1));

            var snpshotStore = new InMemorySnapshootStore(NcqrsEnvironment.Get<IEventStore>() as ISnapshootEventStore,
                                                          new InMemoryEventStore());
            // key param for storing im memory
            NcqrsEnvironment.SetDefault<ISnapshotStore>(snpshotStore);

            NcqrsEnvironment.SetDefault<IAggregateSnapshotter>(
                new CommitedAggregateSnapshotter(NcqrsEnvironment.Get<IAggregateSnapshotter>()));

            var bus = new InProcessEventBus(true);
            NcqrsEnvironment.SetDefault<IEventBus>(bus);
            kernel.Bind<IEventBus>().ToConstant(bus);

#if !MONODROID
            RegisterEventHandlers(bus, kernel);
#endif
        }

        public static void EnsureReadLayerIsBuilt()
        {
            if (!IsReadLayerBuilt)
            {
                lock (lockObject)
                {
                    if (!IsReadLayerBuilt)
                    {
                        RebuildReadLayer();
                    }
                }
            }
        }

        /// <summary>
        /// The rebuild read layer.
        /// </summary>
        /// <exception cref="Exception">
        /// </exception>
        public static void RebuildReadLayer()
        {
            LogManager.GetLogger(typeof(NcqrsInit)).Info("Read layer rebuilding started.");

            var eventBus = NcqrsEnvironment.Get<IEventBus>();
            if (eventBus == null)
            {
                throw new Exception("IEventBus is not properly initialized.");
            }

            #warning hello to Vitaliy Balabanov: rebuild read layer by event sources
            var eventStore = NcqrsEnvironment.Get<IStreamableEventStore>();

            if (eventStore == null)
            {
                throw new Exception("IStreamableEventStore is not correctly initialized.");
            }

            // store.CreateIndex();
            // var myEvents = store.GetAllEvents();
            eventBus.Publish(eventStore.GetEventStream().Select(evnt => evnt as IPublishableEvent));

            isReadLayerBuilt = true;

            LogManager.GetLogger(typeof(NcqrsInit)).Info("Read layer rebuilding finished.");
        }



        #endregion

        #region Methods

        /// <summary>
        /// The initialize command service.
        /// </summary>
        /// <param name="commandSupplier">
        /// The command supplier.
        /// </param>
        /// <returns>
        /// The <see cref="ICommandService"/>.
        /// </returns>
        private static ICommandService InitializeCommandService(ICommandListSupplier commandSupplier)
        {
            var mapper = new AttributeBasedCommandMapper();
            var service = new ConcurrencyResolveCommandService();
            foreach (Type type in commandSupplier.GetCommandList())
            {

                service.RegisterExecutor(type, new UoWMappedCommandExecutor(mapper));
            }

            service.RegisterExecutor(typeof(CreateSnapshotForAR),
                                    new UoWMappedCommandExecutor(new SnapshotCommandMapper()));
        
            return service;
        }

#if !MONODROID
        /// <summary>
        /// The initialize event store.
        /// </summary>
        /// <param name="store">
        /// The store.
        /// </param>
        /// <returns>
        /// The <see cref="IStreamableEventStore"/>.
        /// </returns>
        private static IStreamableEventStore InitializeEventStore(DocumentStore store, int pageSize)
        {
            return new RavenDBEventStore(store, pageSize);
        }

#endif


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