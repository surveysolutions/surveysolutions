using System;

namespace WB.Core.GenericSubdomains.Utils.Services
{
    public interface IRestServiceSettings
    {
        string Endpoint { get; set; }
        TimeSpan Timeout { get; set; }
        int BufferSize { get; set; }
    }
}