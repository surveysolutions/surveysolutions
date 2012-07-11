using Ncqrs;
using System;
using Ninject;
using System.Linq;
using Raven.Client;
using System.Threading;
using Ninject.Web.Common;
using System.ServiceModel;
using Ncqrs.Eventing.Storage;
using RavenQuestionnaire.Core;
using Ncqrs.Commanding.ServiceModel;
using System.Collections.Concurrent;
using Ncqrs.Eventing.Storage.RavenDB;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Commanding.CommandExecution.Mapping;
using RavenQuestionnaire.Core.Commands.Location;
using RavenQuestionnaire.Core.Commands.Questionnaire;
using RavenQuestionnaire.Core.Commands.Synchronization;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Commands.Questionnaire.Group;
using RavenQuestionnaire.Core.Commands.Questionnaire.Question;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;


namespace QApp
{
    public static class Initializer
    {

        #region Properties

        public static IKernel Kernel { get; private set; }

        #endregion

        #region Method

        public static void Init()
        {
            Kernel = CreateKernel();
        }

        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel(new CoreRegistry("http://localhost:8080", false));
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            RegisterServices(kernel);
            NCQRSInit.Init("http://localhost:8080", kernel);
            return kernel;
        }

        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<IDocumentSession>().ToMethod(
               context => context.Kernel.Get<IDocumentStore>().OpenSession()).When(
                   b => OperationContext.Current != null).InScope(o => OperationContext.Current);
            kernel.Bind<IDocumentSession>().ToMethod(
                context => context.Kernel.Get<IDocumentStore>().OpenSession()).When(
                    b =>OperationContext.Current == null).InScope(o => Thread.CurrentThread);
            kernel.Bind<IDocumentSession>().ToMethod(
             context => context.Kernel.Get<IDocumentStore>().OpenSession()).When(
                 b => OperationContext.Current != null).InScope(o => OperationContext.Current);
        }

        #endregion
    }


    public static class NCQRSInit
    {
        public static void Init(string repositoryPath, IKernel kernel)
        {
            NcqrsEnvironment.SetDefault<IEventStore>(InitializeEventStore(repositoryPath));
            NcqrsEnvironment.SetDefault<ICommandService>(InitializeCommandService());

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
                        kernel.GetAll(typeof(IEventHandler<>).MakeGenericType(eventDataType)).Where(
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
            service.RegisterExecutor(typeof(CreateQuestionnaireCommand), new UoWMappedCommandExecutor(mapper));
            service.RegisterExecutor(typeof(CreateLocationCommand), new UoWMappedCommandExecutor(mapper));
            service.RegisterExecutor(typeof(CreateCompleteQuestionnaireCommand), new UoWMappedCommandExecutor(mapper));
            service.RegisterExecutor(typeof(AddGroupCommand), new UoWMappedCommandExecutor(mapper));
            service.RegisterExecutor(typeof(AddQuestionCommand), new UoWMappedCommandExecutor(mapper));
            service.RegisterExecutor(typeof(SetAnswerCommand), new UoWMappedCommandExecutor(mapper));
            service.RegisterExecutor(typeof(AddPropagatableGroupCommand), new UoWMappedCommandExecutor(mapper));
            service.RegisterExecutor(typeof(DeletePropagatableGroupCommand), new UoWMappedCommandExecutor(mapper));
            service.RegisterExecutor(typeof(SetCommentCommand), new UoWMappedCommandExecutor(mapper));
            service.RegisterExecutor(typeof(PreLoadCompleteQuestionnaireCommand), new UoWMappedCommandExecutor(mapper));
            service.RegisterExecutor(typeof(DeleteCompleteQuestionnaireCommand), new UoWMappedCommandExecutor(mapper));
            service.RegisterExecutor(typeof(ChangeStatusCommand), new UoWMappedCommandExecutor(mapper));
            service.RegisterExecutor(typeof(PushEventsCommand), new UoWMappedCommandExecutor(mapper));
            service.RegisterExecutor(typeof(CreateNewSynchronizationProcessCommand), new UoWMappedCommandExecutor(mapper));
            service.RegisterExecutor(typeof(ChangeEventStatusCommand), new UoWMappedCommandExecutor(mapper));
            service.RegisterExecutor(typeof(EndProcessComand), new UoWMappedCommandExecutor(mapper));
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
