using System;
using System.Linq;
using Ncqrs;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using Ncqrs.Eventing.Storage.RavenDB;
using Ninject;
using Raven.Client;
using RavenQuestionnaire.Core.Commands.Location;
using RavenQuestionnaire.Core.Commands.Questionnaire;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Commands.Questionnaire.Group;
using RavenQuestionnaire.Core.Commands.Questionnaire.Question;
using RavenQuestionnaire.Core.Commands.Synchronization;
using RavenQuestionnaire.Core.Services;

namespace RavenQuestionnaire.Web.App_Start
{
    public static class NCQRSInit
    {
        public static void Init(string repositoryPath, IKernel kernel)
        {
            NcqrsEnvironment.SetDefault<IEventStore>(InitializeEventStore(repositoryPath));
            NcqrsEnvironment.SetDefault<ICommandService>(InitializeCommandService());
        
            NcqrsEnvironment.SetDefault<ISnapshottingPolicy>(new SimpleSnapshottingPolicy(1));
            NcqrsEnvironment.SetDefault<ISnapshotStore>(new InMemoryEventStore());
            NcqrsEnvironment.SetDefault<IFileStorageService>(new RavenFileStorageService(kernel.Get<IDocumentStore>()));
            var bus = new InProcessEventBus(true);
            RegisterEventHandlers(bus, kernel);
           
            NcqrsEnvironment.SetDefault<IEventBus>(bus);
        }
        static void RegisterEventHandlers(InProcessEventBus bus, IKernel kernel)
        {
            foreach (var type in typeof(NCQRSInit).Assembly.GetTypes().Where(ImplementsAtLeastOneIEventHandlerInterface))
            {
                foreach (var handlerInterfaceType in type.GetInterfaces().Where(IsIEventHandlerInterface))
                {
                    var eventDataType = handlerInterfaceType.GetGenericArguments().First();
                    Type type1 = type;
                    var handlers =
                        kernel.GetAll(typeof (IEventHandler<>).MakeGenericType(eventDataType)).Where(
                            i => i.GetType() == type1);
                    foreach (object handler in handlers)
                    {
                        bus.RegisterHandler(handler, eventDataType);
                    }
                }
            }
           
        }
        private static bool ImplementsAtLeastOneIEventHandlerInterface(Type type)
        {
            return type.IsClass && !type.IsAbstract &&
                   type.GetInterfaces().Any(IsIEventHandlerInterface);
        }
        private static bool ImplementsAtLeastOneICommand(Type type)
        {
            return type.IsClass && !type.IsAbstract &&
                      type.GetInterfaces().Any(IsICommandInterface);
        }
        private static bool IsICommandInterface(Type type)
        {
            return type.IsInterface &&
                   typeof (ICommand).IsAssignableFrom(type);
        }

        private static bool IsIEventHandlerInterface(Type type)
        {
            return type.IsInterface &&
                   type.IsGenericType &&
                   type.GetGenericTypeDefinition() == typeof(IEventHandler<>);
        }
        private static ICommandService InitializeCommandService()
        {
            var mapper = new AttributeBasedCommandMapper();
            var service = new CommandService();
            foreach (var type in typeof(NCQRSInit).Assembly.GetTypes().Where(ImplementsAtLeastOneICommand))
            {
                service.RegisterExecutor(type, new UoWMappedCommandExecutor(mapper));
            }
            return service;
        }

        private static IEventStore InitializeEventStore(string storePath)
        {
            var eventStore = new RavenDBEventStore(storePath);
            return eventStore;
        }
        
        public static void RebuildReadLayer()
        {
            var myEventBus = NcqrsEnvironment.Get<IEventBus>();
            if (myEventBus == null) 
                throw new Exception("IEventBus is not properly initialized.");
            var myEventStore = NcqrsEnvironment.Get<IEventStore>() as RavenDBEventStore;// as MsSqlServerEventStore;

            if (myEventStore == null)
                throw new Exception("IEventStore is not correct.");

            var myEvents = myEventStore.ReadFrom(DateTime.MinValue);
            myEventBus.Publish(myEvents);

        }

    }
}
