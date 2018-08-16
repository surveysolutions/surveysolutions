using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Core;
using WB.Core.Infrastructure.EventBus;

namespace WB.Core.Infrastructure.Modularity.Autofac
{

    public class AutofacModuleContext : IModuleContext
    {
        const string TargetTypeParameterName = "Autofac.AutowiringPropertyInjector.InstanceType";

        private readonly IComponentContext ctx;
        private readonly IEnumerable<Parameter> parameters;

        public AutofacModuleContext(IComponentContext ctx, IEnumerable<Parameter> parameters)
        {
            this.ctx = ctx;
            this.parameters = parameters;
        }

        public T Resolve<T>()
        {
            return ctx.Resolve<T>();
        }

        public Type MemberDeclaringType
        {
            get
            {
                var targetType = parameters.OfType<NamedParameter>()
                    .FirstOrDefault(np => np.Name == TargetTypeParameterName && np.Value is Type);

                return (Type) targetType?.Value;
            }
        }

        public T Inject<T>()
        {
            return Resolve<T>();
        }

        public T Get<T>()
        {
            return Resolve<T>();
        }

        public T Get<T>(string name)
        {
            return Resolve<T>();
        }

        public object Get(Type type)
        {
            return this.ctx.Resolve(type);
        }

        public object GetServiceWithGenericType(Type type, params Type[] genericType)
        {
            return ctx.Resolve(type.MakeGenericType(genericType));
        }

        public Type GetGenericArgument()
        {
            var targetType = parameters.OfType<NamedParameter>()
                .FirstOrDefault(np => np.Value is Type);

            return (Type) targetType?.Value;
        }

        public Type[] GetGenericArguments()
        {
            return parameters.OfType<NamedParameter>().Select(np => np.Value).OfType<Type>().ToArray();
        }
    }

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
            containerBuilder.RegisterType(implementation).As(@interface);
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
                registrationBuilder.WithParameter(constructorArgument.Name, constructorArgument.Value);
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

        public void BindGeneric(Type implemenation)
        {
            containerBuilder.RegisterGeneric(implemenation);
        }

        public void Unbind<T>()
        {
        }

        public bool HasBinding<T>()
        {
            return false; 
        }

        public void BindInIsolatedThreadScopeOrRequestScopeOrThreadScope<T>()
        {
            throw new NotImplementedException();
        }

        public void BindInIsolatedThreadScopeOrRequestScopeOrThreadScope<T1, T2>() where T2 : T1
        {
            throw new NotImplementedException();
        }


        void IIocRegistry.BindAsSingleton<TInterface, TImplementation>()
        {
            containerBuilder.RegisterType<TImplementation>().As<TInterface>().SingleInstance();
        }

        void IIocRegistry.BindAsSingleton<TInterface1, TInterface2, TImplementation>()
        {
            containerBuilder.RegisterType<TImplementation>().As<TInterface1, TInterface2>().SingleInstance();
        }

        void IIocRegistry.BindAsSingletonWithConstructorArgument<TInterface, TImplementation>(string argumentName, object argumentValue)
        {
            containerBuilder.RegisterType<TImplementation>().As<TInterface>()
                .WithParameter(argumentName, argumentValue).SingleInstance();
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

        public void BindToConstant<T>(Func<IModuleContext, T> func)
        {
            throw new NotImplementedException();
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
