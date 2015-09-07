using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.UI.Tester.Infrastructure.Internals.Rest
{
    interface ITesterNetworkService : INetworkService
    {
        bool IsEndpointReachable();
    }
}
