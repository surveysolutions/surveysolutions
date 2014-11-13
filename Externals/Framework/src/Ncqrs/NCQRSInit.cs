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
        public static void InitializeCommandService(ICommandListSupplier commandSupplier, CommandService service)
        {
            var mapper = new AttributeBasedCommandMapper();
            foreach (Type type in commandSupplier.GetCommandList())
            {

                service.RegisterExecutor(type, new UoWMappedCommandExecutor(mapper));
            }
            NcqrsEnvironment.SetDefault<ICommandService>(service);
        }
    }
}