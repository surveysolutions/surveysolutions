﻿using System.Threading.Tasks;
using Quartz;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;

namespace WB.Core.BoundedContexts.Headquarters.QuartzIntegration
{
    public class QuartzModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.BindAsSingleton<ISchedulerFactory, AutofacSchedulerFactory>();
            registry.BindToMethodInSingletonScope<IScheduler>(ctx => ctx.Get<ISchedulerFactory>().GetScheduler().Result);
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            return Task.CompletedTask;
        }
    }
}
