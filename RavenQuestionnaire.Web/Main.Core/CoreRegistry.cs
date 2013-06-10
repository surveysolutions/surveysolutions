// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CoreRegistry.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The core registry.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Main.Core.Conventions;
using Main.Core.EventHandlers;
using Main.Core.ExpressionExecutors;
using Main.Core.View;
using Main.DenormalizerStorage;
using Ncqrs.Commanding;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Restoring.EventStapshoot;
using Ninject;
using Ninject.Activation;
using Ninject.Extensions.Conventions;
using Ninject.Modules;

using WB.Core.Infrastructure;

#if !MONODROID
using Raven.Client;
using Raven.Client.Document;

using WB.Core.Infrastructure;

#endif

namespace Main.Core
{
    using Ninject.Planning.Bindings;

    /// <summary>
    /// The core registry.
    /// </summary>
    public abstract class CoreRegistry : NinjectModule
    {
        private readonly bool isEmbeded;
        private readonly string repositoryPath;
        private readonly string username;
        private readonly string password;
        private readonly string defaultDatabase;

        // private bool _isWeb;

        public CoreRegistry(string repositoryPath, bool isEmbeded, string username = null, string password = null, string defaultDatabase = null)
        {
            this.repositoryPath = repositoryPath;
            this.isEmbeded = isEmbeded;
            this.username = username;
            this.password = password;
            this.defaultDatabase = defaultDatabase;

            // _isWeb = isWeb;
        }

        #region Public Methods and Operators

        public virtual IEnumerable<Assembly> GetAssweblysForRegister()
        {
            return new[] {(typeof (CoreRegistry)).Assembly, typeof(IDenormalizer).Assembly};

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

        /// <summary>
        /// The load.
        /// </summary>
        public override void Load()
        {
            RegisterDenormalizers();
            RegisterEventHandlers();
            RegisterViewFactories();
            RegisterAdditionalElements();
        }

        #endregion

        #region virtual methods
        protected virtual IEnumerable<Type> RegisteredCommandList()
        {
            var implementations =
             GetAssweblysForRegister().SelectMany(a => a.GetTypes()).Where(t => ImplementsAtLeastOneInterface(t, typeof(ICommand)));
            return implementations;
        }

        protected virtual void RegisterAdditionalElements()
        {
#if !MONODROID
            var storeProvider = new DocumentStoreProvider(this.repositoryPath, this.defaultDatabase, this.isEmbeded, this.username, this.password);
            this.Bind<DocumentStoreProvider>().ToConstant(storeProvider);
            this.Bind<DocumentStore>().ToProvider<DocumentStoreProvider>();
#endif


            ICommandListSupplier commands = new CommandListSupplier(RegisteredCommandList());
            this.Bind<ICommandListSupplier>().ToConstant(commands);

            this.Kernel.Bind(
                x =>
                x.From(GetAssweblysForRegister()).SelectAllInterfaces().Excluding<ICommandListSupplier>().BindWith(
                    new RegisterFirstInstanceOfInterface(GetAssweblysForRegister())));

            foreach (KeyValuePair<Type, Type> customBindType in this.GetTypesForRegistration())
            {
                this.Kernel.Bind(customBindType.Key).To(customBindType.Value);
            }
        }

        protected virtual void RegisterViewFactories()
        {
            BindInterface(GetAssweblysForRegister(),typeof (IViewFactory<,>), (c) => Guid.NewGuid());
        }

        protected virtual void RegisterEventHandlers()
        {
            BindInterface(GetAssweblysForRegister().Union(new Assembly[]{typeof(SnapshootLoaded).Assembly}), typeof(IEventHandler<>), (c) => this.Kernel);
        }

        protected virtual void RegisterDenormalizers()
        {
            foreach (
                var denormalizer in
                    GetAssweblysForRegister()
                                             .SelectMany(a => a.GetTypes())
                                             .Where(DenormalizerStorageImplementation))
            {

                this.Kernel.Bind(denormalizer).ToSelf().InSingletonScope();
            }
            this.Kernel.Bind(typeof (IDenormalizerStorage<>)).ToMethod(GetStorage);
            this.Kernel.Bind(typeof (IQueryableDenormalizerStorage<>)).ToMethod(GetStorage);
        }

        private bool DenormalizerStorageImplementation(Type t)
        {
            if (t.IsInterface || t.IsAbstract)
                return false;
            return t.GetInterfaces().FirstOrDefault(i => i.IsGenericType &&
                                                         i.GetGenericTypeDefinition() == typeof (IDenormalizerStorage<>)) !=
                   null;
        }

        protected virtual object GetStorage(IContext context)
        {
            var genericParameter = context.GenericArguments[0];

         /*   #if !MONODROID

            if(genericParameter.GetCustomAttributes(typeof(SmartDenormalizerAttribute), true).Length > 0)
                return Kernel.Get(typeof(PersistentDenormalizer<>).MakeGenericType(genericParameter));

            else
            #endif*/
                return this.Kernel.Get(typeof(InMemoryDenormalizer<>).MakeGenericType(genericParameter));
        }

        #endregion
        protected void BindInterface(IEnumerable<Assembly> assembyes,Type interfaceType, Func<IContext,object> scope)
        {

            var implementations =
             assembyes.SelectMany(a => a.GetTypes()).Where(t => ImplementsAtLeastOneInterface(t, interfaceType));
            foreach (Type implementation in implementations)
            {

                this.Kernel.Bind(interfaceType).To(implementation).InScope(scope);
                if (interfaceType.IsGenericType)
                {
                    var interfaceImplementations =
                        implementation.GetInterfaces().Where(i => IsInterfaceInterface(i, interfaceType));
                    foreach (Type interfaceImplementation in interfaceImplementations)
                    {
                        this.Kernel.Bind(interfaceType.MakeGenericType(interfaceImplementation.GetGenericArguments())).
                            To(
                                implementation).InScope(scope);
                    }
                }
            }
        }
       
        private bool ImplementsAtLeastOneInterface(Type type, Type interfaceType)
        {
            return type.IsClass && !type.IsAbstract &&
                   type.GetInterfaces().Any(i => IsInterfaceInterface(i, interfaceType));
        }

        private  bool IsInterfaceInterface(Type type, Type interfaceType)
        {
            return type.IsInterface &&
                   ((interfaceType.IsGenericType && type.IsGenericType &&
                     type.GetGenericTypeDefinition() == interfaceType) ||
                    (!type.IsGenericType && !interfaceType.IsGenericType && type==interfaceType));
        }
    }
}