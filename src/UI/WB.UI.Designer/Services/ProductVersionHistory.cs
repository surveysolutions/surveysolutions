using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.Infrastructure.Versions;

namespace WB.UI.Designer.Services
{
    internal class ProductVersionHistory : IProductVersionHistory
    {
        private readonly IProductVersion productVersion;
        private readonly DesignerDbContext dbContext;

        public ProductVersionHistory(IProductVersion productVersion,
            DesignerDbContext dbContext)
        {
            this.productVersion = productVersion;
            this.dbContext = dbContext;
        }

        private string CurrentVersion => this.productVersion.ToString();

        public void RegisterCurrentVersion()
        {
            if (this.GetLastRegisteredVersion() == this.CurrentVersion)
                return;

            this.dbContext.ProductVersionChanges.Add(new ProductVersionChange(this.CurrentVersion, DateTime.UtcNow));

            this.dbContext.SaveChanges();
        }

        private string? GetLastRegisteredVersion()
            => this.dbContext.ProductVersionChanges.OrderByDescending(change => change.UpdateTimeUtc)
                .FirstOrDefault()?.ProductVersion;

        public IEnumerable<ProductVersionChange> GetHistory()
                => this.dbContext.ProductVersionChanges.OrderByDescending(change => change.UpdateTimeUtc);
    }
}
