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
using Android.Content;
using AndroidNcqrs.Eventing.Storage.SQLite;
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
using Ncqrs.Restoring.EventStapshoot;
using Ninject;

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
            NcqrsEnvironment.SetDefault(kernel.Get<IEventStore>());

            NcqrsEnvironment.SetDefault(InitializeCommandService());

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
		public static void RebuildReadLayer()
		{
			var myEventBus = NcqrsEnvironment.Get<IEventBus>();
			if (myEventBus == null)
			{
				throw new Exception("IEventBus is not properly initialized.");
			}

			var myEventStore = NcqrsEnvironment.Get<IEventStore>() as SQLiteEventStore; // as MsSqlServerEventStore;

			if (myEventStore == null)
			{
				throw new Exception("IEventStore is not correct.");
			}

	        var myEvents = myEventStore.GetAllEvents();
			myEventBus.Publish(myEvents
				.Select(evt => evt as IPublishableEvent)
				.ToList());
			/* foreach (IGrouping<Guid, CommittedEvent> eventsByAggregateRoot in myEvents.GroupBy(x => x.EventSourceId))
		{
			myEventBus.Publish(ExcludeHistoryBefaourSnapshoot(eventsByAggregateRoot));
		}*/
		}
		private static IEnumerable<CommittedEvent> ExcludeHistoryBefaourSnapshoot(IEnumerable<CommittedEvent> events)
		{
			var lastSnapshoot = events.LastOrDefault(x => x.Payload is SnapshootLoaded);
			if (lastSnapshoot == null)
				return events;
			else
				return events.SkipWhile(x => x != lastSnapshoot);

		}

        #endregion

        #region Methods

        /// <summary>
        /// The implements at least one i command.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        private static bool ImplementsAtLeastOneICommand(Type type)
        {
            return type.IsClass && !type.IsAbstract && type.GetInterfaces().Any(IsICommandInterface);
        }

        /// <summary>
        /// The implements at least one i event handler interface.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        private static bool ImplementsAtLeastOneIEventHandlerInterface(Type type)
        {
            return type.IsClass && !type.IsAbstract && type.GetInterfaces().Any(IsIEventHandlerInterface);
        }

        /// <summary>
        /// The initialize command service.
        /// </summary>
        /// <returns>
        /// The Ncqrs.Commanding.ServiceModel.ICommandService.
        /// </returns>
        private static ICommandService InitializeCommandService()
        {
            var mapper = new AttributeBasedCommandMapper();
            var service = new ConcurrencyResolveCommandService();

            foreach (Type type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(ImplementsAtLeastOneICommand))
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
		//private static IEventStore InitializeEventStore(Context applicationContext)
		//{
		//    var eventStore = new SQLiteEventStore(applicationContext);
		//    return eventStore;
		//}

        /// <summary>
        /// The is i command interface.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        private static bool IsICommandInterface(Type type)
        {
            return type.IsInterface && typeof (ICommand).IsAssignableFrom(type);
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

            foreach (Type type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(ImplementsAtLeastOneIEventHandlerInterface))
            {
                foreach (Type handlerInterfaceType in type.GetInterfaces().Where(IsIEventHandlerInterface))
                {
                    Type eventDataType = handlerInterfaceType.GetGenericArguments().First();
                    Type type1 = type;
                    IEnumerable<object> handlers =
                        kernel.GetAll(typeof (IEventHandler<>).MakeGenericType(eventDataType)).Where(
                            i => i.GetType() == type1);
                    foreach (object handler in handlers)
                    {
                        bus.RegisterHandler(handler, eventDataType);
                    }
                }
            }
        }

        #endregion
    }
}
