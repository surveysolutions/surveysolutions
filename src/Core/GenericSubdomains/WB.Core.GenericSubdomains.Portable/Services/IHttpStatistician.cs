using System;
using Flurl.Http;

namespace WB.Core.GenericSubdomains.Portable.Services
{
    public interface IHttpStatistician
    {
        void Reset();
        HttpStats GetStats();
        void CollectHttpCallStatistics(HttpCall call);
    }
}