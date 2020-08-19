using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.HttpServices.HttpClient;

namespace WB.Core.Infrastructure.HttpServices.Services
{
    public interface IHttpStatistician
    {
        void Reset();
        HttpStats GetStats();
        void CollectHttpCallStatistics(HttpCall httpCall);
    }
}