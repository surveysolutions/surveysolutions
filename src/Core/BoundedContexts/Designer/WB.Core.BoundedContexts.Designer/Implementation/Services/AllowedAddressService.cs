using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.AllowedAddresses;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    public class AllowedAddressService : IAllowedAddressService
    {
        private readonly IPlainStorageAccessor<AllowedAddress> allowedAddressStorage;

        public AllowedAddressService(IPlainStorageAccessor<AllowedAddress> allowedAddressStorage)
        {
            this.allowedAddressStorage = allowedAddressStorage;
        }

        public IEnumerable<AllowedAddress> GetAddresses()
        {
            return this.allowedAddressStorage.Query(_ => _.ToList());
        }

        public AllowedAddress GetAddressById(int id)
        {
            return this.allowedAddressStorage.GetById(id);
        }

        public void Update(AllowedAddress allowedAddress)
        {
            this.allowedAddressStorage.Store(allowedAddress, allowedAddress.Id);
        }

        public void Remove(int id)
        {
            this.allowedAddressStorage.Remove(id);
        }

        public void Add(AllowedAddress allowedAddress)
        {
            this.allowedAddressStorage.Store(allowedAddress, Guid.NewGuid().FormatGuid());
        }

        public bool IsAllowedAddress(IPAddress ip)
        {
            var allowedAddresses = this.allowedAddressStorage.Query(_ => _.Select(x => x.Address).ToList());
            return allowedAddresses.Any(x => x.Equals(ip));
        }
    }
}