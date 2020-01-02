using System;
using System.Collections.Generic;
using System.Text;

namespace WB.Core.Infrastructure.Modularity
{
    public static class ServiceCollectionExtensions
    {
        public static void AddTransient<TInterface, TService>(this IIocRegistry registry) where TService : TInterface
        {
            registry.Bind<TInterface, TService>();
        }

        public static void AddScoped<TService>(this IIocRegistry registry, Func<IServiceProvider, TService> implementationFactory)
         where TService : class
        {
            registry.BindToMethod(ctx => implementationFactory(ctx.Get<IServiceProvider>()));
        }

        public static void AddScoped<TService>(this IIocRegistry registry)
            where TService : class
        {
            registry.BindInPerLifetimeScope<TService, TService>();
        }
    }
}
