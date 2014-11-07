using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs.Commanding.CommandExecution.Mapping;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using Ninject;
using WB.Core.Infrastructure.CommandBus;
using CommandService = Ncqrs.Commanding.ServiceModel.CommandService;

namespace Ncqrs
{
    public static class NcqrsInit
    {
        public static void Init(IKernel kernel)
        {
            NcqrsEnvironment.SetDefault<ISnapshottingPolicy>(new SimpleSnapshottingPolicy(1));

            var snpshotStore = new InMemoryEventStore();
            // key param for storing im memory
            NcqrsEnvironment.SetDefault<ISnapshotStore>(snpshotStore);

            var bus = new InProcessEventBus(true);
            NcqrsEnvironment.SetDefault<IEventBus>(bus);
            kernel.Bind<IEventBus>().ToConstant(bus);

            RegisterEventHandlers(bus, kernel);
        }

        public static void InitializeCommandService(ICommandListSupplier commandSupplier, CommandService service)
        {
            var mapper = new AttributeBasedCommandMapper();
            foreach (Type type in commandSupplier.GetCommandList())
            {

                service.RegisterExecutor(type, new UoWMappedCommandExecutor(mapper));
            }
            NcqrsEnvironment.SetDefault<ICommandService>(service);
        }

        private static bool IsIEventHandlerInterface(Type type)
        {
            return type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEventHandler<>);
        }

        internal static void RegisterEventHandlers(InProcessEventBus bus, IKernel kernel)
        {
            IEnumerable<object> handlers = Enumerable.Distinct<object>(kernel.GetAll(typeof(IEventHandler<>))).ToList();
            foreach (object handler in handlers)
            {
                IEnumerable<Type> ieventHandlers = handler.GetType().GetInterfaces().Where(IsIEventHandlerInterface);
                foreach (Type ieventHandler in ieventHandlers)
                {
                    bus.RegisterHandler(handler, ieventHandler.GetGenericArguments()[0]);
                }
            }
        }
    }
}