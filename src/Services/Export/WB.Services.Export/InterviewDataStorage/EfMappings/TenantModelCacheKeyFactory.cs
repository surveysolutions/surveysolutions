using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using WB.Services.Export.Infrastructure;

namespace WB.Services.Export.InterviewDataStorage.EfMappings
{
    public class TenantModelCacheKeyFactory : IModelCacheKeyFactory
    {
        public object Create(DbContext context, bool designTime)
        {
            var dbContext = context as TenantDbContext;

            return new {
                Type = context.GetType(),
                Schema = dbContext?.TenantContext.Tenant.SchemaName()
            };
        }
    }
}
