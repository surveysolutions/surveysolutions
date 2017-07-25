using WB.Core.GenericSubdomains.Portable.Implementation;


namespace WB.Core.GenericSubdomains.Portable.Services
{
    public interface IHttpStatistician
    {
        void Reset();
        HttpStats GetStats();
        void CollectHttpCallStatistics(HttpCall httpCall);
    }
}