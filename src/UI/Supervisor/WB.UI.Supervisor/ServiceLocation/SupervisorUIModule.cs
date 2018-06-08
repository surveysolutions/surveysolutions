using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;

namespace WB.UI.Supervisor.ServiceLocation
{
    public class SupervisorUIModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<SupervisorMvxApplication>();
            registry.Bind<SupervisorAppStart>();
        }

        public Task Init(IServiceLocator serviceLocator) => Task.CompletedTask;
    }
}
