using System.Collections.Generic;
using System.Linq;
using System.Net;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.AllowedAddresses;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    public class AllowedAddressService : IAllowedAddressService
    {
        private readonly DesignerDbContext dbContext;

        public AllowedAddressService(DesignerDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public IEnumerable<AllowedAddress> GetAddresses()
        {
            return this.dbContext.AllowedAddress.ToList();
        }

        public AllowedAddress GetAddressById(int id)
        {
            return this.dbContext.AllowedAddress.Find(id);
        }

        public void Update(AllowedAddress allowedAddress)
        {
            this.dbContext.AllowedAddress.Update(allowedAddress);
        }

        public void Remove(int id)
        {
            var existing = dbContext.AllowedAddress.Find(id);
            if (existing != null)
            {
                this.dbContext.AllowedAddress.Remove(existing);
            }
        }

        public void Add(AllowedAddress allowedAddress)
        {
            this.dbContext.AllowedAddress.Add(allowedAddress);
        }

        public bool IsAllowedAddress(IPAddress ip)
        {
            var allowedAddresses = this.dbContext.AllowedAddress.Select(x => x.Address).ToList();
            return allowedAddresses.Any(x => x.Equals(ip));
        }
    }
}
