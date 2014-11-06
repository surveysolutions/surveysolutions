using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Commands;
using Microsoft.Practices.ServiceLocation;
using Ncqrs;
using Ncqrs.Commanding.CommandExecution.Mapping;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using Ninject;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.CommandBus;

namespace Main.Core
{
    #if MONODROID
    using AndroidNcqrs.Eventing.Storage.SQLite;
#endif


    //using Ncqrs.Eventing.Storage.RavenDB;

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
#if MONODROID
        //    NcqrsEnvironment.SetDefault(InitializeCommandService(kernel.Get<ICommandListSupplier>()));
            NcqrsEnvironment.SetDefault(kernel.Get<IEventStore>());
            InitializeCommandService(kernel.Get<ICommandListSupplier>(), new ConcurrencyResolveCommandService(ServiceLocator.Current.GetInstance<ILogger>()));

#else

            InitializeCommandService(kernel.Get<ICommandListSupplier>(), new ConcurrencyResolveCommandService(ServiceLocator.Current.GetInstance<ILogger>()));

#endif


            NcqrsEnvironment.SetDefault<ISnapshottingPolicy>(new SimpleSnapshottingPolicy(1));

            var snpshotStore = new InMemoryEventStore();
            // key param for storing im memory
            NcqrsEnvironment.SetDefault<ISnapshotStore>(snpshotStore);

            var bus = new InProcessEventBus(true);
            NcqrsEnvironment.SetDefault<IEventBus>(bus);
            kernel.Bind<IEventBus>().ToConstant(bus);

#if !MONODROID
            RegisterEventHandlers(bus, kernel);
#endif
        }

        #endregion

        #region Methods

        public static void InitializeCommandService(ICommandListSupplier commandSupplier, CommandService service)
        {
            var mapper = new AttributeBasedCommandMapper();
       //     var service = new ConcurrencyResolveCommandService(ServiceLocator.Current.GetInstance<ILogger>());
            foreach (Type type in commandSupplier.GetCommandList())
            {

                service.RegisterExecutor(type, new UoWMappedCommandExecutor(mapper));
            }
            NcqrsEnvironment.SetDefault<ICommandService>(service);
          //  return service;
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
        internal static void RegisterEventHandlers(InProcessEventBus bus, IKernel kernel)
        {
            IEnumerable<object> handlers = kernel.GetAll(typeof(IEventHandler<>)).Distinct().ToList();
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