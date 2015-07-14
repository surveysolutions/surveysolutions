using System;

namespace WB.Core.BoundedContexts.Tester.Infrastructure
{
    public interface ISettingsProvider
    {
        string Endpoint { get; }
        TimeSpan RequestTimeout { get; }
        int BufferSize { get; }
        bool AcceptUnsignedSslCertificate { get; }

        int GpsReceiveTimeoutSec { get; }
    }
}