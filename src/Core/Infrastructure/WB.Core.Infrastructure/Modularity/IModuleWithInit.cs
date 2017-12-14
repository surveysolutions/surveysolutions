using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.Core.Infrastructure.Modularity
{
    public interface IModuleWithInit : IModule
    {
        void Init(IServiceLocator serviceLocator);
    }
}