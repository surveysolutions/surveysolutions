using System;

namespace ApiUtil
{
    public class ConsoleRestServiceSettings
    {
        public TimeSpan Timeout => new TimeSpan(0, 0, 0, 30);

        public int BufferSize => 512;

        public bool AcceptUnsignedSslCertificate => false;
    }
}