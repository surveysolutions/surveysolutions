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
using Ninject;
using Ninject.Activation;
using Ninject.Extensions.Conventions;
using Ninject.Modules;

#if !MONODROID
using Raven.Client;
using Raven.Client.Document;
#endif

namespace Main.Core
{
    using Ninject.Planning.Bindings;

    /// <summary>
    /// The core registry.
    /// </summary>
    public abstract class CoreRegistry : NinjectModule
    {
        #region Fields

        /// <summary>
        /// The _is embeded.
        /// </summary>
        private readonly bool isEmbeded;

        /// <summary>
        /// The _repository path.
        /// </summary>
        private readonly string repositoryPath;

        #endregion

        // private bool _isWeb;
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreRegistry"/> class.
        /// </summary>
        /// <param name="repositoryPath">
        /// The repository path.
        /// </param>
        /// <param name="isEmbeded">
        /// The is embeded.
        /// </param>
        public CoreRegistry(string repositoryPath, bool isEmbeded)
        {
            this.repositoryPath = repositoryPath;
            this.isEmbeded = isEmbeded;

            // _isWeb = isWeb;
        }

        #endregion

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
            var storeProvider = new DocumentStoreProvider(this.repositoryPath, this.isEmbeded);
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
            BindInterface(typeof (IViewFactory<,>), (c) => Guid.NewGuid());
        }

        protected virtual void RegisterEventHandlers()
        {
            BindInterface(typeof (IEventHandler<>), (c) => this.Kernel);
        }

        protected virtual void RegisterDenormalizers()
        {
            this.Kernel.Bind(
                x =>
                x.From(GetAssweblysForRegister()).Select(
                    t =>
                    t.GetInterfaces().FirstOrDefault(
                        i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof (IDenormalizerStorage<>)) != null)
                    .BindToSelf().Configure(binding => binding.InSingletonScope()));
            this.Kernel.Bind(
                x =>
                x.From(GetAssweblysForRegister()).Select(
                    t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof (DenormalizerStorageProvider<>)).
                    BindToSelf().Configure(binding => binding.InSingletonScope()));
            Bind(typeof (IDenormalizerStorage<>)).ToMethod(ActivteDenormalizerFromProvider);

        }

        #endregion
        protected void BindInterface(Type interfaceType, Func<IContext,object> scope)
        {

            var implementations =
             GetAssweblysForRegister().SelectMany(a => a.GetTypes()).Where(t => ImplementsAtLeastOneInterface(t, interfaceType));
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
             /*   else{
                    this.Kernel.Bind(interfaceType).To(implementation);
                }*/
            }
        }

        /// <summary>
        /// The implements at least one i event handler interface.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        private bool ImplementsAtLeastOneInterface(Type type, Type interfaceType)
        {
            return type.IsClass && !type.IsAbstract &&
                   type.GetInterfaces().Any(i => IsInterfaceInterface(i, interfaceType));
        }
        /// <summary>
        /// The is i event handler interface.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        private  bool IsInterfaceInterface(Type type, Type interfaceType)
        {
            return type.IsInterface &&
                   ((interfaceType.IsGenericType && type.IsGenericType &&
                     type.GetGenericTypeDefinition() == interfaceType) ||
                    (!type.IsGenericType && !interfaceType.IsGenericType && type==interfaceType));
        }
      
        protected IEnumerable<Type> ProcessDenormalizer(Assembly assembly)
        {
            return assembly.GetTypes().Where(t => t.GetInterfaces().FirstOrDefault(
                i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof (IDenormalizerStorage<>)) != null);
        }
        protected object ActivteDenormalizerFromProvider(IContext ctx)
        {
            return (ctx.Kernel.Get(typeof (DenormalizerStorageProvider<>).MakeGenericType(ctx.GenericArguments)) as IProvider).Create(ctx);
        }
    }
}