using System.Collections.Generic;
using System.Net;
using WB.Core.BoundedContexts.Designer.Views.AllowedAddresses;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IAllowedAddressService
    {
        IEnumerable<AllowedAddress> GetAddresses();
        AllowedAddress GetAddressById(int id);
        void Update(AllowedAddress allowedAddress);
        void Remove(int id);
        void Add(AllowedAddress allowedAddress);
        bool IsAllowedAddress(IPAddress ip);
    }
}
