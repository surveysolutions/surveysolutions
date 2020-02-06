using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace WB.Services.Export.Tests.WithDatabase
{
    public class TenantDbContextTest
    {
        [Test]
        public void ensure_that_multiple_same_tenant_tenantDbContext_use_reference_equal_connection_string()
        {
            string GetConnectionString()
            {
                var ctx = Create.NpgsqlTenantDbContext(tenantName: "test");
                var _ = ctx.ChangeTracker; // will trigger TenantDbContext.OnConfigure method
                return ctx.Database.GetDbConnection().ConnectionString;
            }

            var cs1 = GetConnectionString();
            var cs2 = GetConnectionString();

            Assert.AreSame(cs1, cs2);
        }
    }
}
