using System;
using WB.Core.BoundedContexts.Tester.Infrastructure;

namespace WB.UI.Tester.Infrastructure
{
    public interface ITesterSettings
    {
        string Endpoint { get; }
        TimeSpan RequestTimeout { get; }
        int BufferSize { get; }
        bool AcceptUnsignedSslCertificate { get; }
    }
}