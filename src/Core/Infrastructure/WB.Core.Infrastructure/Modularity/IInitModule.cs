using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.Core.Infrastructure.Modularity
{
    public interface IInitModule
    {
        void Init(IServiceLocator serviceLocator);
    }
}