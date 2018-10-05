using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Core;
using Autofac.Core.Lifetime;

namespace WB.Core.Infrastructure.Modularity.Autofac
{
    public class AutofacModuleAdapter : AutofacModuleAdapter<IIocRegistry>
    {
        public AutofacModuleAdapter(IModule<IIocRegistry> module) : base(module)
        {
        }
    }

    public abstract class AutofacModuleAdapter<TIoc> : Module, IIocRegistry where TIoc : class, IIocRegistry 
    {
        protected readonly IModule<TIoc> module;
        protected ContainerBuilder containerBuilder;

        protected AutofacModuleAdapter(IModule<TIoc> module)
        {
            this.module = module;
        }

        protected override void Load(ContainerBuilder builder)
        {
            containerBuilder = builder;
            this.module.Load(this as TIoc);
        }

        void IIocRegistry.Bind<TInterface, TImplementation>()
        {
            containerBuilder.RegisterType<TImplementation>().As<TInterface>();
        }

        public void Bind(Type @interface, Type implementation)
        {
            if (@interface.IsGenericType && implementation.IsGenericType)
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
            var registrationBuilder = containerBuilder.RegisterType<TImplementation>().As<TInterface>();
            
            foreach (var constructorArgument in constructorArguments)
            {
                //var value = constructorArgument.Value.Invoke(null);

                //object Callback(IContext context) => constructorArgument.Value.Invoke(new NinjectModuleContext(context));
                registrationBuilder.WithParameter(
                    new ResolvedParameter(
                        (pi, ctx) => pi.Name == constructorArgument.Name, //pi.ParameterType == typeof(string) &&
                        (pi, ctx) => constructorArgument.Value.Invoke(new AutofacModuleContext(ctx, new List<Parameter>() )))
                        //constructorArgument.Value.Invoke(new AutofacModuleContext(ctx, new List<Parameter>())) 

                    //constructorArgument.Name, constructorArgument.Value.Invoke()
                    );
            }
        }

        public void Bind<TImplementation>()
        {
            containerBuilder.RegisterType<TImplementation>();
        }

        public void BindWithConstructorArgument<TInterface, TImplementation>(string argumentName, object argumentValue) where TImplementation : TInterface
        {
            containerBuilder.RegisterType<TImplementation>().As<TInterface>()
                .WithParameter(argumentName, argumentValue);
        }

        public void BindInPerUnitOfWorkOrPerRequestScope<TInterface, TImplementation>() where TImplementation : TInterface
        {
            containerBuilder.RegisterType<TImplementation>().As<TInterface>()
                .InstancePerMatchingLifetimeScope(
                    AutofacServiceLocatorAdapterWithChildrenScopes.UnitOfWorkScope,
                    MatchingScopeLifetimeTags.RequestLifetimeScopeTag);
        }

        public void BindWithConstructorArgumentInPerLifetimeScope<TInterface, TImplementation>(string argumentName, object argumentValue) where TImplementation : TInterface
        {
            containerBuilder.RegisterType<TImplementation>().As<TInterface>()
                .WithParameter(argumentName, argumentValue).InstancePerLifetimeScope();
        }

        public void BindGeneric(Type implementation)
        {
            containerBuilder.RegisterGeneric(implementation);
        }

        public void BindInPerLifetimeScope<T1, T2>() where T2 : T1
        {
            containerBuilder.RegisterType<T2>().As<T1>().InstancePerLifetimeScope();
        }

        void IIocRegistry.BindAsSingleton<TInterface, TImplementation>()
        {
            containerBuilder.RegisterType<TImplementation>().As<TInterface>().SingleInstance();
        }

        public void BindAsSingleton<TInterface1, TInterface2, TImplementation>() where TImplementation : TInterface2, TInterface1
        {
            containerBuilder.RegisterType<TImplementation>().As<TInterface1, TInterface2>().SingleInstance();
        }

        public void BindAsSingleton<TInterface1, TInterface2, TInterface3, TImplementation>() where TImplementation : TInterface3, TInterface2, TInterface1
        {
            containerBuilder.RegisterType<TImplementation>().As<TInterface1, TInterface2, TInterface3>().SingleInstance();
        }

        void IIocRegistry.BindAsSingletonWithConstructorArgument<TInterface, TImplementation>(string argumentName, object argumentValue)
        {
            containerBuilder.RegisterType<TImplementation>().As<TInterface>()
                .WithParameter(argumentName, argumentValue).SingleInstance();
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
            containerBuilder.Register((ctx, p) => func(new AutofacModuleContext(ctx, p)));
        }

        public void BindToMethodInSingletonScope<T>(Func<IModuleContext, T> func, string named = null)
        {
            containerBuilder.Register((ctx, p) => func(new AutofacModuleContext(ctx, p))).SingleInstance();
        }

        public void BindToMethodInSingletonScope(Type @interface, Func<IModuleContext, object> func)
        {
            containerBuilder.Register((ctx, p) => func(new AutofacModuleContext(ctx, p))).SingleInstance();
        }

        public void BindToMethodInRequestScope<T>(Func<IModuleContext, T> func)
        {
            containerBuilder.Register((ctx, p) => func(new AutofacModuleContext(ctx, p))).InstancePerRequest();
        }

        public void BindToMethod<T>(Func<T> func)
        {
            containerBuilder.Register(ctx => func());
        }

        public void BindToConstant<T>(Func<T> func)
        {
            containerBuilder.Register(ctx => func()).SingleInstance();
        }

        public void BindToConstant<T>(Func<IModuleContext, T> func)
        {
            containerBuilder.Register((ctx, p) => func(new AutofacModuleContext(ctx, p))).SingleInstance();
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
