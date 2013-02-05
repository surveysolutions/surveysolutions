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
using Main.Core;
using Ncqrs.Eventing.Storage;
using Ninject.Activation;

namespace AndroidApp.Injections
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
            this.Bind<IEventStore>().To<SQLiteEventStore>();
            this.Unbind<IAuthentication>();
            
            var membership = new AndroidAuthentication();
            this.Bind<IAuthentication>().ToConstant(membership);

            this.Unbind<IProjectionStorage>();
            this.Bind<IProjectionStorage>().ToMethod(CreateStorage).
                InScope(c => CapiApplication.Context);
        }
        protected IProjectionStorage CreateStorage(IContext c)
        {
            return new InternalProjectionStorage(CapiApplication.Context);
        }
    }
}
