using System.Collections.Generic;
using System.Linq;
using System.Net;
using NHibernate.Linq;
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

        public IEnumerable<AllowedAddress> GetAddresses() => this.dbContext.AllowedAddress.ToList();

        public AllowedAddress GetAddressById(int id) => this.dbContext.AllowedAddress.Find(id);

        public void Update(AllowedAddress allowedAddress)
        {
            this.dbContext.AllowedAddress.Update(allowedAddress);
            this.dbContext.SaveChanges();
        }

        public void Remove(int id)
        {
            var address = this.dbContext.AllowedAddress.Find(id);
            if (address == null) return;

            this.dbContext.AllowedAddress.Remove(address);
            this.dbContext.SaveChanges();
        }

        public void Add(AllowedAddress allowedAddress)
        {
            this.dbContext.AllowedAddress.Add(allowedAddress);
            this.dbContext.SaveChanges();
        }

        public bool IsAllowedAddress(IPAddress ip) => this.dbContext.AllowedAddress.Any(x => x.Address == ip.ToString());
    }
}
