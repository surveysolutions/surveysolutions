// -----------------------------------------------------------------------
// <copyright file="AndroidCoreRegistry.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AndroidApp.Core.Model.Authorization;
using AndroidApp.Core.Model.ProjectionStorage;
using AndroidApp.Core.Model.ViewModel.Dashboard;
using AndroidNcqrs.Eventing.Storage.SQLite;
using Core.CAPI.Synchronization;
using Main.Core.View.User;
using Main.DenormalizerStorage;
using Ncqrs.Eventing.Storage;
using Ninject;
using Ninject.Activation;

namespace AndroidApp.Injections
{
    using Main.Core;
    using SynchronizationMessages.WcfInfrastructure;
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class AndroidCoreRegistry : CoreRegistry
    {
        public AndroidCoreRegistry(string repositoryPath, bool isEmbeded) : base(repositoryPath, isEmbeded)
        {
        }
        /// <summary>
        /// The get assweblys for register.
        /// </summary>
        /// <returns>
        /// </returns>
        public override IEnumerable<Assembly> GetAssweblysForRegister()
        {
            return
                Enumerable.Concat(base.GetAssweblysForRegister(), new[] { typeof(ClientEventStreamReader).Assembly, typeof(DashboardModel).Assembly, GetType().Assembly });
        }
        public override void Load()
        {
            base.Load();
            this.Bind<IEventStore>().To<SQLiteEventStore>();
            this.Unbind<IAuthentication>();

            var membership = new AndroidAuthentication(Kernel.Get<IDenormalizerStorage<UserView>>());
            this.Bind<IAuthentication>().ToConstant(membership);

            this.Bind<IChanelFactoryWrapper>().To<ChanelFactoryWrapper>();
            this.Unbind<IProjectionStorage>();
            this.Bind<IProjectionStorage>().ToMethod(CreateStorage).
                InScope(c => CapiApplication.Context);

            this.Bind<IChanelFactoryWrapper>().To<ChanelFactoryWrapper>();                
        }
        protected IProjectionStorage CreateStorage(IContext c)
        {
            return new InternalProjectionStorage(CapiApplication.Context);
        }
    }
}
