using System.Collections.Generic;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public class GetQuestionnaireListResponse : ICommunicationMessage
    {
        public List<string> Questionnaires { get; set; } = new List<string>();
    }

    public class SendBigAmountOfDataRequest : ICommunicationMessage
    {
        public byte[] Data { get; set; }
    }

    public class SendBigAmountOfDataResponse : ICommunicationMessage
    {
        public byte[] Data { get; set; }
    }
}
