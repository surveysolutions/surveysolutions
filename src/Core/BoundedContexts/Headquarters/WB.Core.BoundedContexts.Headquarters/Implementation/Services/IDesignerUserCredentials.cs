using WB.Core.Infrastructure.HttpServices.HttpClient;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    public interface IDesignerUserCredentials
    {
        RestCredentials Get();
        void Set(RestCredentials restCredentials);
        void SetTaskCredentials(RestCredentials restCredentials);
    }
}
