using System.Net;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IIpAddressProvider
    {
        IPAddress GetClientIpAddress();
    }
}
