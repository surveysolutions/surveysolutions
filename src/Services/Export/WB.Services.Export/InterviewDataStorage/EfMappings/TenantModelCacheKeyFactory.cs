using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using WB.Services.Export.Infrastructure;

namespace WB.Services.Export.InterviewDataStorage.EfMappings
{
    public class TenantModelCacheKeyFactory : IModelCacheKeyFactory
    {
        public object Create(DbContext context) => Create(context, false);
        
        public object Create(DbContext context, bool designTime)
        {
            if (context is not TenantDbContext dbContext)
                return context.GetType();

            return (
                context.GetType(),
                dbContext.TenantContext.Tenant.SchemaName(),
                designTime
            );
        }
    }
}
