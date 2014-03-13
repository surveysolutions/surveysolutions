using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Commands;
using Ncqrs;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using Ninject;
using Ninject.Modules;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.FunctionalDenormalization;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.EventDispatcher;

namespace WB.UI.Headquarters
{
    internal class CqrsModule : NinjectModule
    {
        public override void Load()
        {
            var commandService = new ConcurrencyResolveCommandService(this.Kernel.Get<ILogger>());
            RegisterCommands(commandService);

            NcqrsEnvironment.SetDefault(commandService);
            NcqrsEnvironment.SetDefault<ICommandService>(commandService);
            this.Kernel.Bind<ICommandService>().ToConstant(commandService);

            NcqrsEnvironment.SetDefault<ISnapshottingPolicy>(new SimpleSnapshottingPolicy(1));
            NcqrsEnvironment.SetDefault<ISnapshotStore>(new InMemoryEventStore());

            this.CreateAndRegisterEventBus();
        }

        private void CreateAndRegisterEventBus()
        {
            var bus = new NcqrCompatibleEventDispatcher();
            NcqrsEnvironment.SetDefault<IEventBus>(bus);

            this.Kernel.Bind<IEventBus>().ToConstant(bus);
            this.Kernel.Bind<IEventDispatcher>().ToConstant(bus);

            List<IEventHandler> eventHandlers = this.Kernel.GetAll<IEventHandler>().ToList();

            foreach (var eventHandler in eventHandlers)
            {
                bus.Register(eventHandler);
            }
        }

        private static void RegisterCommands(CommandService commandService)
        {
            var mapper = new AttributeBasedCommandMapper();

            IEnumerable<Type> commands = typeof (HeadquartersBoundedContextModule).Assembly.GetTypes().Where(IsCommand).ToList();

            foreach (Type type in commands)
            {
                commandService.RegisterExecutor(type, new UoWMappedCommandExecutor(mapper));
            }
        }

        private static bool IsCommand(Type type)
        {
            return type.GetInterfaces().Contains(typeof (ICommand)) && !type.IsAbstract;
        }
    }
}