using Ncqrs;
using Ncqrs.Commanding.CommandExecution.Mapping;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using Ncqrs.Commanding.ServiceModel;
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
            /*
              Kernel.Register(
                Component
                    .For<IAggregateRootCreationStrategy>()
                    .ImplementedBy<DynamicSnapshotAggregateRootCreationStrategy>(),
                Component
                    .For<IAggregateSupportsSnapshotValidator>()
                    .ImplementedBy<AggregateSupportsDynamicSnapshotValidator>(),
                Component
                    .For<IAggregateSnapshotter>()
                    .ImplementedBy<AggregateDynamicSnapshotter>(),
                Component
                    .For<IDynamicSnapshotAssembly>()
                    .ImplementedBy<DynamicSnapshotAssembly>()
                    .OnCreate((kernel, instance) =>
                        {
                            if (_generateDynamicSnapshotAssembly)
                                instance.CreateAssemblyFrom(_assemblyWithAggreagateRoots);
                        }),
                Component.For<SnapshotableAggregateRootFactory>(),
                Component.For<DynamicSnapshotAssemblyBuilder>(),
                Component.For<DynamicSnapshotTypeBuilder>(),
                Component.For<SnapshotableImplementerFactory>());
             */
            /*    IWindsorContainer container = new WindsorContainer();
                container.AddFacility("ncqrs.ds", new DynamicSnapshotFacility(asm));
                container.Register(
                    Component.For<ISnapshottingPolicy>().ImplementedBy<SimpleSnapshottingPolicy>(),
                    Component.For<IEventStore>().Forward<ISnapshotStore>().Instance(dsa),
                    Component.For<CompleteQuestionnaireAR>().AsSnapshotable()
                    );


                WindsorConfiguration config = new WindsorConfiguration(container);
             */
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
