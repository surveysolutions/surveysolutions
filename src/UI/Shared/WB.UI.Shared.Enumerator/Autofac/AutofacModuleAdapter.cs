using System;
using Autofac;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Modularity;

namespace WB.UI.Shared.Enumerator.Services.Ninject
{

    /// <summary>
    ///  TODO: maybe delete
    /// </summary>
    public class AutofacModuleAdapter : Module, IIocRegistry
    {
        private readonly IModule module;
        private ContainerBuilder containerBuilder;

        public AutofacModuleAdapter(IModule module)
        {
            this.module = module;
        }

        protected override void Load(ContainerBuilder builder)
        {
            containerBuilder = builder;
            this.module.Load(this);
        }

        void IIocRegistry.Bind<TInterface, TImplementation>()
        {
            containerBuilder.RegisterType<TImplementation>().As<TInterface>();
        }

        public void Bind(Type @interface, Type implementation)
        {
            if (@interface.IsGenericType)
            {
                containerBuilder.RegisterGeneric(implementation).As(@interface);
            }
            else
            {
                containerBuilder.RegisterType(implementation).As(@interface);
            }
        }

        public void Bind<TInterface1, TInterface2, TImplementation>() where TImplementation : TInterface1, TInterface2
        {
            containerBuilder.RegisterType<TImplementation>().As<TInterface1, TInterface2>();
        }

        public void Bind<TInterface, TImplementation>(params ConstructorArgument[] constructorArguments) where TImplementation : TInterface
        {
            throw new NotImplementedException();
        }

        public void Bind<TImplementation>()
        {
            containerBuilder.RegisterType<TImplementation>();
        }

        public void BindWithConstructorArgument<TInterface, TImplementation>(string argumentName, object argumentValue) where TImplementation : TInterface
        {
            throw new NotImplementedException();
        }

        public void BindGeneric(Type implemenation)
        {
            containerBuilder.RegisterGeneric(implemenation);
        }

        public void RegisterDenormalizer<T>() where T : IEventHandler
        {
            throw new NotImplementedException();
        }

        public void Unbind<T>()
        {
            throw new NotImplementedException();
        }

        public bool HasBinding<T>()
        {
            throw new NotImplementedException();
        }

        public void BindToSelfInSingletonScopeWithConstructorArgument(Type[] types, string argumentName, Func<IModuleContext, object> argumentValue)
        {
            throw new NotImplementedException();
        }

        public void BindInIsolatedThreadScopeOrRequestScopeOrThreadScope<T>()
        {
            throw new NotImplementedException();
        }

        void IIocRegistry.BindAsSingleton<TInterface, TImplementation>()
        {
            containerBuilder.RegisterType<TImplementation>().As<TInterface>().SingleInstance();
        }

        void IIocRegistry.BindAsSingletonWithConstructorArgument<TInterface, TImplementation>(string argumentName, object argumentValue)
        {
            containerBuilder.RegisterType<TImplementation>().As<TInterface>()
                .WithParameter(argumentName, argumentValue);
        }

        public void BindAsSingletonWithConstructorArgument<TInterface, TImplementation>(
            params ConstructorArgument[] constructorArguments) where TImplementation : TInterface
        {
            throw new NotImplementedException();
            /*var registrationBuilder = containerBuilder.RegisterType<TImplementation>().As<TInterface>();
            foreach (var constructorArgument in constructorArguments)
            {
                registrationBuilder.WithParameter(constructorArgument.Name, constructorArgument.Value(null));
            }*/
        }

        void IIocRegistry.BindToRegisteredInterface<TInterface, TRegisteredInterface>()
        {
            containerBuilder.Register<TInterface>(c => c.Resolve<TRegisteredInterface>());
        }

        public void BindToMethod<T>(Func<T> func, string name = null)
        {
            if (name == null)
            {
                BindToMethod(func);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void BindToMethod<T>(Func<IModuleContext, T> func, string name = null)
        {
            throw new NotImplementedException();
        }

        public void BindToMethodInSingletonScope<T>(Func<IModuleContext, T> func, string named = null)
        {
            throw new NotImplementedException();
        }

        public void BindToMethodInSingletonScope(Type @interface, Func<IModuleContext, object> func)
        {
            throw new NotImplementedException();
        }

        public void BindToMethodInRequestScope<T>(Func<IModuleContext, T> func)
        {
            throw new NotImplementedException();
        }

        public void BindToMethod<T>(Func<T> func)
        {
            containerBuilder.Register(ctx => func());
        }

        public void BindToMethod<T>(Func<IModuleContext, T> func)
        {
            throw new NotImplementedException();
        }

        public void BindToConstant<T>(Func<T> func)
        {
            containerBuilder.Register(ctx => func()).SingleInstance();
        }

        public void BindToConstructorInSingletonScope<T>(Func<IConstructorContext, T> func)
        {
            throw new NotImplementedException();
        }

        public void BindToConstructorInSingletonScope<T>(Func<IModuleContext, T> func)
        {
            throw new NotImplementedException();
        }

        public void BindAsSingleton(Type @interface, Type implementation)
        {
            if (@interface.IsGenericType)
            {
                containerBuilder.RegisterGeneric(implementation).As(@interface).SingleInstance();
            }
            else
            {
                containerBuilder.RegisterType(implementation).As(@interface).SingleInstance();
            }
        }
    }
}