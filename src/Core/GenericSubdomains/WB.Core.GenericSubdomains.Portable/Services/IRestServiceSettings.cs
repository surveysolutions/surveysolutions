using System;

namespace WB.Core.GenericSubdomains.Portable.Services
{
    public interface IRestServiceSettings
    {
        string Endpoint { get;  }
        TimeSpan Timeout { get;  }
        int BufferSize { get;  }
        bool AcceptUnsignedSslCertificate { get;  }
    }
}