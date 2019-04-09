using System;
using Microsoft.Extensions.DependencyInjection;
using WB.Core.Infrastructure.DependencyInjection;

namespace WB.UI.Designer1.DependencyInjection
{
    public class DependencyRegistry : IDependencyRegistry
    {
        private readonly IServiceCollection services;

        public DependencyRegistry(IServiceCollection services)
        {
            this.services = services;
        }

        public void Bind<TInterface, TImplementation>() where TImplementation : class, TInterface where TInterface : class
        {
            services.AddTransient<TInterface, TImplementation>();
        }

        public void Bind(Type @interface, Type implementation)
        {
            services.AddTransient(@interface, implementation);
        }

        public void BindAsSingleton<TInterface, TImplementation>() where TImplementation : class, TInterface where TInterface : class
        {
            services.AddSingleton<TInterface, TImplementation>();
        }

        public void BindAsSingleton<TInterface, TImplementation>(TImplementation instance) where TInterface : class where TImplementation : class, TInterface
        {
            services.AddSingleton<TInterface, TImplementation>(sp => instance);
        }

        public void BindAsScoped<TInterface, TImplementation>() where TImplementation : class, TInterface where TInterface : class
        {
            services.AddScoped<TInterface, TImplementation>();
        }
    }
}
