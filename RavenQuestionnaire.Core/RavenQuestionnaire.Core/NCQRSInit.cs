using System;
using System.Linq;
using Ncqrs;
using Ncqrs.Commanding.CommandExecution.Mapping;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using Ncqrs.Eventing.Storage.RavenDB;
using Ninject;
using RavenQuestionnaire.Core.Commands.Location;
using RavenQuestionnaire.Core.Commands.Questionnaire;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Commands.Questionnaire.Group;
using RavenQuestionnaire.Core.Commands.Questionnaire.Question;
using RavenQuestionnaire.Core.Domain;
using RavenQuestionnaire.Core.Events;
using RavenQuestionnaire.Core.Events.Questionnaire.Completed;

namespace RavenQuestionnaire.Web.App_Start
{
    public static class NCQRSInit
    {
        public static void Init(string repositoryPath, IKernel kernel)
        {
            NcqrsEnvironment.SetDefault<IEventStore>(InitializeEventStore(repositoryPath));
            NcqrsEnvironment.SetDefault<ICommandService>(InitializeCommandService());
        //    NcqrsEnvironment.SetDefault<IEventStore>(new InMemoryEventStore());
            NcqrsEnvironment.SetDefault<ISnapshottingPolicy>(new SimpleSnapshottingPolicy(1));
            NcqrsEnvironment.SetDefault<ISnapshotStore>(new InMemoryEventStore());

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

            //add assembly scan to register executors 
            service.RegisterExecutor(typeof(CreateQuestionnaireCommand), new UoWMappedCommandExecutor(mapper));
            service.RegisterExecutor(typeof(CreateLocationCommand), new UoWMappedCommandExecutor(mapper));
            service.RegisterExecutor(typeof(CreateCompleteQuestionnaireCommand), new UoWMappedCommandExecutor(mapper));

            service.RegisterExecutor(typeof(AddGroupCommand), new UoWMappedCommandExecutor(mapper));
            service.RegisterExecutor(typeof(AddQuestionCommand), new UoWMappedCommandExecutor(mapper));
            service.RegisterExecutor(typeof(SetAnswerCommand), new UoWMappedCommandExecutor(mapper));

            service.RegisterExecutor(typeof(AddPropagatableGroupCommand), new UoWMappedCommandExecutor(mapper));
            service.RegisterExecutor(typeof(DeletePropagatableGroupCommand), new UoWMappedCommandExecutor(mapper));
            

            return service;
        }

        private static IEventStore InitializeEventStore(string storePath)
        {
            var eventStore = new RavenDBEventStore(storePath);
            return eventStore;
        }
    }
}