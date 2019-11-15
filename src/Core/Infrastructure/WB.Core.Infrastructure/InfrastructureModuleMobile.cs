﻿using System.Threading.Tasks;
using Ncqrs;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using WB.Core.Infrastructure.DependencyInjection;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.WriteSide;

namespace WB.Core.Infrastructure
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class InfrastructureModuleMobile : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<IEventSourcedAggregateRootRepositoryWithCache, EventSourcedAggregateRootRepositoryWithCache>();
            registry.BindToRegisteredInterface<IEventSourcedAggregateRootRepository, IEventSourcedAggregateRootRepositoryWithCache>(); 
            registry.BindToRegisteredInterface<IEventSourcedAggregateRootRepositoryCacheCleaner, IEventSourcedAggregateRootRepositoryWithCache>(); 
            registry.Bind<IPlainAggregateRootRepository, PlainAggregateRootRepository>();
            registry.BindAsSingleton<IAggregateLock, AggregateLock>();
            registry.BindAsSingleton<ICommandsMonitoring, TraceCommandsMonitoring>();
            registry.BindAsSingleton<IDenormalizerRegistry, DenormalizerRegistry>();
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status) => Task.CompletedTask;
    }
}
