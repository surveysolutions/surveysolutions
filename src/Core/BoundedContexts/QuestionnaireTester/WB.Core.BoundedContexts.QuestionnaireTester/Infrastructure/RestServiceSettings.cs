using System;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure
{
    public class RestServiceSettings
    {
        public string Endpoint { get; set; }
        public TimeSpan Timeout { get; set; }
        public int BufferSize { get; set; }
        public bool AcceptUnsignedSslCertificate { get; set; }
    }
}