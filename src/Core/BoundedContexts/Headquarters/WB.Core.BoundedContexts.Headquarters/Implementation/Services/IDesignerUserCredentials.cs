using WB.Core.GenericSubdomains.Portable.Implementation;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    public interface IDesignerUserCredentials
    {
        RestCredentials Get();
        void Set(RestCredentials restCredentials);
    }
}
