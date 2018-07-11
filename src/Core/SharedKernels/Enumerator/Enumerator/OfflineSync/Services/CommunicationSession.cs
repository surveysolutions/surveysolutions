using System;
using System.Threading;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services
{
    public class CommunicationSession : IDisposable
    {
        public CommunicationSession()
        {
            Session.Value = this;
        }

        public Guid SessionId { get; } = Guid.NewGuid();

        private static AsyncLocal<CommunicationSession> Session { get; } = new AsyncLocal<CommunicationSession>();

        public static CommunicationSession Current => Session.Value ?? new CommunicationSession();


        public void Dispose()
        {
            // store HAR file for this session
        }

        public void BytesSend(long headerBytesLongLength)
        {
            BytesSendTotal += headerBytesLongLength;
        }

        
        public long BytesSendTotal { get; set; } = 0;
        public long RequestsTotal { get; set; } = 0;
    }
}
