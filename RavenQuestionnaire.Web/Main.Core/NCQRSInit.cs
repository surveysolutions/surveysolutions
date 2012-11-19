// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NCQRSInit.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The ncqrs init.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Commands;
using Main.Core.Services;
using Main.DenormalizerStorage;
using Ncqrs;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using Ncqrs.Eventing.Storage.RavenDB;
using Ncqrs.Restoring.EventStapshoot;
using Ncqrs.Restoring.EventStapshoot.EventStores.RavenDB;
using Ninject;
using Ninject.Parameters;
using Ninject.Planning.Bindings;
using Raven.Client.Document;

namespace Main.Core
{
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
            NcqrsEnvironment.SetDefault(InitializeEventStore(kernel.Get<DocumentStore>()));

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
        /// <exception cref="Exception">
        /// </exception>
        public static void RebuildReadLayer(IKernel kernel)
        {
            var store = kernel.Get<DocumentStore>();
            store.Conventions = new DocumentConvention
                {
                    JsonContractResolver = new PropertiesOnlyContractResolver(),
                    FindTypeTagName = x => "Events"
                    //NewDocumentETagGenerator = GenerateETag
                };

            var myEventBus = NcqrsEnvironment.Get<IEventBus>();
            if (myEventBus == null)
            {
                throw new Exception("IEventBus is not properly initialized.");
            }

            var myEventStore = NcqrsEnvironment.Get<IEventStore>() as RavenDBEventStore; // as MsSqlServerEventStore;

            if (myEventStore == null)
            {
                throw new Exception("IEventStore is not correct.");
            }
            store.CreateIndex();
            var myEvents = store.GetAllEvents();
            myEventBus.Publish(myEvents);
        }

        #endregion

        #region Methods


        

        /// <summary>
        /// The initialize command service.
        /// </summary>
        /// <returns>
        /// The Ncqrs.Commanding.ServiceModel.ICommandService.
        /// </returns>
        private static ICommandService InitializeCommandService(ICommandListSupplier commandSupplier)
        {
            var mapper = new AttributeBasedCommandMapper();
            var service = new ConcurrencyResolveCommandService();

            foreach (var type in commandSupplier.GetCommandList())
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
        private static IEventStore InitializeEventStore(DocumentStore store)
        {
            var eventStore = new RavenDBEventStore(store);
            return eventStore;
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
            return type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == typeof (IEventHandler<>);
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
            IEnumerable<object> handlers =
                kernel.GetAll(typeof (IEventHandler<>));
            foreach (object handler in handlers)
            {
                var ieventHandlers = handler.GetType().GetInterfaces().Where(IsIEventHandlerInterface);
                foreach (Type ieventHandler in ieventHandlers)
                {

                    bus.RegisterHandler(handler, ieventHandler.GetGenericArguments()[0]);
                }

            }
        }

        #endregion
    }
}
