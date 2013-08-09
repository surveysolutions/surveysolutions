using System;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Supervisor.CompleteQuestionnaireDenormalizer
{
    public class ZipView : IView
    {
        public ZipView(Guid publicKey, string payload, long sequence)
        {
            PublicKey = publicKey;
            Payload = payload;
            Sequence = sequence;
        }

        public Guid PublicKey { get; private set; }
        public string Payload { get; private set; }
        public long Sequence { get; private set; }
    }
}