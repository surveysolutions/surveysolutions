﻿using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Implementation.CommandBus;
using WB.Core.Infrastructure.Modularity;

namespace WB.Core.Infrastructure
{
    public class InfrastructureModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<ICommandService, CommandService>();
        }
    }
}