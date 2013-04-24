// -----------------------------------------------------------------------
// <copyright file="AndroidCoreRegistry.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AndroidNcqrs.Eventing.Storage.SQLite;
using CAPI.Android.Core.Model.Authorization;
using CAPI.Android.Core.Model.ProjectionStorage;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using Core.CAPI.Synchronization;
using Main.Core.View.User;
using Main.DenormalizerStorage;
using Ncqrs.Eventing.Storage;
using Ninject;
using Ninject.Activation;
using Main.Core;
using SynchronizationMessages.WcfInfrastructure;

namespace CAPI.Android.Injections
{
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

            this.Bind<IEventStore>().ToConstant(new MvvmCrossSqliteEventStore());
            this.Unbind<IAuthentication>();

           

            this.Bind<IChanelFactoryWrapper>().To<ChanelFactoryWrapper>();
            this.Unbind<IProjectionStorage>();
            this.Bind<IProjectionStorage>().ToMethod(CreateStorage).
                InScope(c => CapiApplication.Context);

            this.Bind<IChanelFactoryWrapper>().To<ChanelFactoryWrapper>();                
        }
        protected IProjectionStorage CreateStorage(IContext c)
        {
            return new InternalProjectionStorage();
        }
    }
}
