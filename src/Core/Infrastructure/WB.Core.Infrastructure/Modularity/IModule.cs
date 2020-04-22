using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.Core.Infrastructure.Modularity
{
    public interface IModule : IModule<IIocRegistry>
    {
    }

    public interface IModule<in T>
    {
        void Load(T registry);
    }
}
