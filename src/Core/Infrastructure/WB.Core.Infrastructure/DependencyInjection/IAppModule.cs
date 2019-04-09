using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;

namespace WB.Core.Infrastructure.DependencyInjection
{
    public interface IAppModule
    {
        void Load(IDependencyRegistry registry);

        Task InitAsync(IServiceLocator serviceLocator, UnderConstructionInfo status);
    }
}
