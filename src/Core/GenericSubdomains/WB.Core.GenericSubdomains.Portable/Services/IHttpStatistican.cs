using System;
using Flurl.Http;

namespace WB.Core.GenericSubdomains.Portable.Services
{
    public interface IHttpStatistican
    {
        void Reset();
        HttpStats GetStats();
        void CollectHttpCallStatistics(HttpCall call);
    }
}