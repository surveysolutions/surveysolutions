using System;

namespace WB.Core.GenericSubdomains.Utils.Services
{
    public interface IRestServiceSettings
    {
        string Endpoint { get;  }
        TimeSpan Timeout { get;  }
        int BufferSize { get;  }
        bool AcceptUnsignedSslCertificate { get;  }
    }
}