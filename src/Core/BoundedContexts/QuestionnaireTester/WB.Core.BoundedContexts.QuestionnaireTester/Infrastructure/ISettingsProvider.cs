using System;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure
{
    public interface ISettingsProvider
    {
        string Endpoint { get; }
        TimeSpan RequestTimeout { get; }
        int BufferSize { get; }
        bool AcceptUnsignedSslCertificate { get; }
    }
}