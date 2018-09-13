using WB.Services.Export.Interview;

namespace WB.Services.Export.Infrastructure
{
    public interface ICache
    {
        object Get(string key, string tenantId);
        void Set(string key, object value, string tenantId);
    }
}
