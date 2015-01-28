using System;

namespace WB.Core.GenericSubdomains.Utils.Services
{
    public interface IRestServiceSettings
    {
        string BaseAddress();
        TimeSpan GetTimeout();
    }
}