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
using Main.Core.View.User;
using Main.DenormalizerStorage;
using Ncqrs.Eventing.Storage;
using Ninject;
using Ninject.Activation;
using Main.Core;

namespace CAPI.Android.Injections
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class AndroidCoreRegistry : CoreRegistry
    {
        private const string EventStoreDatabaseName = "EventStore";

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
                Enumerable.Concat(base.GetAssweblysForRegister(), new[] { typeof(DashboardModel).Assembly, GetType().Assembly });
        }
        public override void Load()
        {
            base.Load();

            this.Bind<IEventStore>().ToConstant(new MvvmCrossSqliteEventStore(EventStoreDatabaseName));
            this.Unbind<IAuthentication>();

            this.Unbind<IProjectionStorage>();
            this.Bind<IProjectionStorage>().ToMethod(CreateStorage).
                InScope(c => CapiApplication.Context);      
        }
        protected IProjectionStorage CreateStorage(IContext c)
        {
            return new InternalProjectionStorage();
        }
    }
}
