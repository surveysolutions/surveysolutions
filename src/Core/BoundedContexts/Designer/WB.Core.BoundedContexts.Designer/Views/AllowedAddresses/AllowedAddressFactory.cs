using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Views.AllowedAddresses
{
    public interface IAllowedAddressService
    {
        IEnumerable<AllowedAddress> GetAddresses();
        AllowedAddress GetAddressById(int id);
        void Update(AllowedAddress allowedAddress);
        void Remove(int id);
        void Add(AllowedAddress allowedAddress);
    }

    public class AllowedAddressService : IAllowedAddressService
    {
        private readonly IPlainStorageAccessor<AllowedAddress> allowedAddressStorage;

        public AllowedAddressService(IPlainStorageAccessor<AllowedAddress> allowedAddressStorage)
        {
            this.allowedAddressStorage = allowedAddressStorage;
        }

        public IEnumerable<AllowedAddress> GetAddresses()
        {
            return allowedAddressStorage.Query(_ => _.ToList());
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
    }
}