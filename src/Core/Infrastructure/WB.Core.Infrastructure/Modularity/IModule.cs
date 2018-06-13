using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.Core.Infrastructure.Modularity
{
    public interface IModule : IInitModule
    {
        void Load(IIocRegistry registry);
    }
}
