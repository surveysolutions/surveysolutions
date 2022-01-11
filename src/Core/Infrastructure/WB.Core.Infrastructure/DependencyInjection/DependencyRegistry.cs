using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Modularity;

namespace WB.Core.Infrastructure.DependencyInjection
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

        public void BindToConstant<TInterface>(Func<IServiceProvider, TInterface> implementation) where TInterface : class
        {
            services.AddSingleton(s => implementation.Invoke(s));
        }

        public void BindToConstant<TInterface>(Func<TInterface> implementation) where TInterface : class
        {
            services.AddSingleton(s => implementation.Invoke());
        }

        public void BindAsSingleton(Type @interface, Type implementation)
        {
            services.AddSingleton(@interface, implementation);
        }

        public void BindToMethod<TInterface>(Func<TInterface> func, string name = null) where TInterface : class
        {
            services.AddTransient(s => func.Invoke());
        }

        public void BindToMethod<TImplementation>(Func<IServiceProvider, TImplementation> func, string name = null) where TImplementation : class
        {
            services.AddTransient(func);
        }

        public void BindToMethodInSingletonScope<T>(Func<IServiceProvider, T> func, string named = null)
        {
            services.AddSingleton(func);
        }

        public void RegisterDenormalizer<T>() where T : IEventHandler
        {
            Type[] eventHandlerTypes = { typeof(IEventHandler), typeof(IEventHandler<>) };
            var denormalizerType = typeof(T);

            this.Bind(typeof(IEventHandler), denormalizerType);

            foreach (var interfaceType in eventHandlerTypes)
            {
                if (!interfaceType.IsGenericType) continue;

                var interfaceImplementations = denormalizerType.GetInterfaces()
                    .Where(t => t.IsGenericType)
                    .Where(t => t.GetGenericTypeDefinition() == interfaceType);

                var genericInterfaceTypes =
                    interfaceImplementations.Select(
                        interfaceImplementation => interfaceType.MakeGenericType(interfaceImplementation.GetGenericArguments()));

                foreach (var genericInterfaceType in genericInterfaceTypes)
                {
                    this.Bind(genericInterfaceType, denormalizerType);
                }
            }
        }
    }
}
