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
using Main.Core.ExpressionExecutors;
using Main.Core.View;
using Main.DenormalizerStorage;
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
        /// The load.
        /// </summary>
        public override void Load()
        {
#if !MONODROID
			var storeProvider = new DocumentStoreProvider(this.repositoryPath, this.isEmbeded);
			this.Bind<DocumentStoreProvider>().ToConstant(storeProvider);
			this.Bind<DocumentStore>().ToProvider<DocumentStoreProvider>().InSingletonScope();

			Bind<IDocumentStore>().ToProvider<DocumentStoreProvider>().InSingletonScope();
#endif
           this.Kernel.Bind(
                x =>
                x.From(GetAssweblysForRegister()).SelectAllClasses().BindWith(
                    new RegisterGenericTypesOfInterface(typeof(IViewFactory<,>))));
            this.Kernel.Bind(
                x =>
                x.From(GetAssweblysForRegister()).SelectAllClasses().BindWith(
                    new RegisterGenericTypesOfInterface(typeof(IExpressionExecutor<,>))));

           
            RegisterDenormalizers();
            this.Kernel.Bind(
                x =>
                x.From(GetAssweblysForRegister()).Select(
                    t =>
                    t.GetInterfaces().FirstOrDefault(
                        i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>)) != null).
                    BindAllInterfaces());

           this.Kernel.Bind(
                x => x.From(GetAssweblysForRegister()).SelectAllInterfaces().BindWith(new RegisterFirstInstanceOfInterface()));

        }

        #endregion

        protected  void RegisterDenormalizers()
        {
            /*  if (typeof (T).GetCustomAttributes(typeof (SmartDenormalizerAttribute), true).Length > 0)
            {
                return this.container.Get<WeakReferenceDenormalizer<T>>();
            }*/
            this.Kernel.Bind(
                 x =>
                  x.From(GetAssweblysForRegister()).Select(
                   t =>
                   t.GetInterfaces().FirstOrDefault(
                       i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDenormalizerStorage<>)) != null).BindToSelf().Configure(binding => binding.InSingletonScope()));
            this.Kernel.Bind(
                 x =>
                  x.From(GetAssweblysForRegister()).Select(
                   t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(DenormalizerStorageProvider<>)).BindToSelf().Configure(binding => binding.InSingletonScope()));
           /* var denormalizerImpls = GetAssweblysForRegister().SelectMany(ProcessDenormalizer);
            foreach (Type denormalizerImpl in denormalizerImpls)
            {*/
            Bind(typeof(IDenormalizerStorage<>)).ToMethod(ActivteDenormalizerFromProvider); 
              /*  this.Kernel.Bind(denormalizerImpl).To(typeof (WeakReferenceDenormalizer<>)).When(
                    f => GetWeekBinding(f.Target.Type));
                this.Kernel.Bind(denormalizerImpl).To(typeof(InMemoryDenormalizer<>)).When(
                 f => !GetWeekBinding(f.Target.Type));*/
         //   }
           

        }
        protected IEnumerable<Type> ProcessDenormalizer(Assembly assembly)
        {
            return assembly.GetTypes().Where(t => t.GetInterfaces().FirstOrDefault(
                i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof (IDenormalizerStorage<>)) != null);
        }
        protected object ActivteDenormalizerFromProvider(IContext ctx)
        {
            return
                (ctx.Kernel.Get(
                    typeof (DenormalizerStorageProvider<>).MakeGenericType(ctx.GenericArguments))
                 as IProvider).Create(ctx);
        }
    }
}