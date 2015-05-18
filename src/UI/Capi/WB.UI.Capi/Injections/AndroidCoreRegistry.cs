// -----------------------------------------------------------------------
// <copyright file="AndroidCoreRegistry.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WB.Core.BoundedContexts.Capi.Views.Login;
using WB.Core.GenericSubdomains.ErrorReporting.Services;
using WB.Core.GenericSubdomains.Utils.Implementation;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.UI.Capi.Settings;

using System;
using Main.DenormalizerStorage;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ninject;
using Ninject.Activation;
using Ninject.Modules;

using WB.Core.GenericSubdomains.Android;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.UI.Capi.Views.Login;

namespace WB.UI.Capi.Injections
{
    public abstract class CoreRegistry : NinjectModule
    {
        protected virtual IEnumerable<Assembly> GetAssembliesForRegistration()
        {
            return new[] { (typeof(CoreRegistry)).Assembly };
        }

        /// <summary>
        /// Gets pairs of interface/type which should be registered.
        /// Usually is used to return implementation of interfaces declared not in assemblies returned by GetAssemblies method.
        /// </summary>
        /// <returns>Pairs of interface/implementation.</returns>
        protected virtual IEnumerable<KeyValuePair<Type, Type>> GetTypesForRegistration()
        {
            return Enumerable.Empty<KeyValuePair<Type, Type>>();
        }

        public override void Load()
        {
            RegisterDenormalizers();
            RegisterEventHandlers();
            RegisterAdditionalElements();
        }

        protected virtual void RegisterAdditionalElements()
        {
            foreach (KeyValuePair<Type, Type> customBindType in this.GetTypesForRegistration())
            {
                this.Kernel.Bind(customBindType.Key).To(customBindType.Value);
            }
        }

        protected virtual void RegisterViewFactories()
        {
            BindInterface(this.GetAssembliesForRegistration(), typeof(IViewFactory<,>), (c) => Guid.NewGuid());
        }

        protected virtual void RegisterEventHandlers()
        {
            BindInterface(this.GetAssembliesForRegistration(), typeof(IEventHandler<>), (c) => this.Kernel);
        }

        protected virtual void RegisterDenormalizers()
        {
            // currently in-memory repo accessor also contains repository itself as internal dictionary, so we need to create him as singletone
            this.Kernel.Bind(typeof(InMemoryReadSideRepositoryAccessor<>)).ToSelf().InSingletonScope();

            this.Kernel.Bind(typeof(IReadSideRepositoryReader<>)).ToMethod(this.GetInMemoryReadSideRepositoryAccessor);
            this.Kernel.Bind(typeof(IQueryableReadSideRepositoryReader<>)).ToMethod(this.GetInMemoryReadSideRepositoryAccessor);
            this.Kernel.Bind(typeof(IReadSideRepositoryWriter<>)).ToMethod(this.GetInMemoryReadSideRepositoryAccessor);
        }

        protected object GetInMemoryReadSideRepositoryAccessor(IContext context)
        {
            var genericParameter = context.GenericArguments[0];

            return this.Kernel.Get(typeof(InMemoryReadSideRepositoryAccessor<>).MakeGenericType(genericParameter));
        }

        protected void BindInterface(IEnumerable<Assembly> assembyes, Type interfaceType, Func<IContext, object> scope)
        {

            var implementations =
             assembyes.SelectMany(a => a.GetTypes()).Where(t => t.IsPublic && ImplementsAtLeastOneInterface(t, interfaceType));
            foreach (Type implementation in implementations)
            {
                if (interfaceType != typeof(IViewFactory<,>))
                {
                    this.Kernel.Bind(interfaceType).To(implementation).InScope(scope);
                }
                if (interfaceType.IsGenericType)
                {
                    var interfaceImplementations =
                        implementation.GetInterfaces().Where(i => IsInterfaceInterface(i, interfaceType));
                    foreach (Type interfaceImplementation in interfaceImplementations)
                    {
                        this.Kernel.Bind(interfaceType.MakeGenericType(interfaceImplementation.GetGenericArguments())).
                            To(implementation).InScope(scope);
                    }
                }
            }
        }

        private bool ImplementsAtLeastOneInterface(Type type, Type interfaceType)
        {
            return type.IsClass && !type.IsAbstract &&
                   type.GetInterfaces().Any(i => IsInterfaceInterface(i, interfaceType));
        }

        private bool IsInterfaceInterface(Type type, Type interfaceType)
        {
            return type.IsInterface
                && ((interfaceType.IsGenericType && type.IsGenericType && type.GetGenericTypeDefinition() == interfaceType)
                    || (!type.IsGenericType && !interfaceType.IsGenericType && type == interfaceType));
        }
    }

    public class AndroidCoreRegistry : CoreRegistry
    {
        protected override IEnumerable<Assembly> GetAssembliesForRegistration()
        {
            return
                Enumerable.Concat(base.GetAssembliesForRegistration(), new[] { typeof(ImportFromSupervisor).Assembly, this.GetType().Assembly });
        }

        public override void Load()
        {
            base.Load();

            this.Bind<JsonUtilsSettings>().ToSelf().InSingletonScope();
            this.Bind<IJsonUtils>().To<NewtonJsonUtils>();
            this.Bind<IStringCompressor>().To<JsonCompressor>();
            this.Bind<IViewFactory<LoginViewInput, LoginView>>().To<LoginViewFactory>();

            this.Bind<IRestServiceSettings>().To<RestServiceSettings>().InSingletonScope();
            this.Bind<IErrorReportingSettings>().To<ErrorReportingSettings>().InSingletonScope();
        }
    }
}
