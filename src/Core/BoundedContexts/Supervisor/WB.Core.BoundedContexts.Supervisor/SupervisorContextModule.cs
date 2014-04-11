using System.Net.Http;
using Ninject.Modules;
using WB.Core.BoundedContexts.Supervisor.Synchronization;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.BoundedContexts.Supervisor.Users;
using WB.Core.BoundedContexts.Supervisor.Users.Implementation;

namespace WB.Core.BoundedContexts.Supervisor
{
    public class SupervisorBoundedContextModule : NinjectModule
    {
        private readonly HeadquartersSettings headquartersSettings;

        public SupervisorBoundedContextModule(HeadquartersSettings headquartersSettings)
        {
            this.headquartersSettings = headquartersSettings;
        }

        public override void Load()
        {
            this.Bind<HeadquartersSettings>().ToConstant(this.headquartersSettings);
            this.Bind<IHeadquartersLoginService>().To<HeadquartersLoginService>();
            this.Bind<ILocalFeedStorage>().To<LocalFeedStorage>();
            this.Bind<ISynchronizer>().To<Synchronizer>();

            this.Bind<HttpMessageHandler>().To<HttpClientHandler>();
        }
    }
}