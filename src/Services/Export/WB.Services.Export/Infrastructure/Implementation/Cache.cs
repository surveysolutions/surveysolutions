using System;
using System.Runtime.Caching;
using WB.Services.Export.Questionnaire;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Infrastructure.Implementation
{
    public class Cache : ICache
    {
        private readonly MemoryCache memoryCache = new MemoryCache("ServiceCache");

        public object Get(QuestionnaireId key, TenantId tenantId)
        {
            return memoryCache.Get(tenantId.ToString() + key);
        }

        public void Set(string key, object value, TenantId tenantId)
        {
            this.memoryCache.Set(tenantId + key, 
                value,
                DateTime.Now.AddHours(1));
        }
    }
}
