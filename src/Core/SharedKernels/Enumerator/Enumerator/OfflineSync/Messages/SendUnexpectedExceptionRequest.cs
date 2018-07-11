using System;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public class SendUnexpectedExceptionRequest : ICommunicationMessage
    {
        public UnexpectedExceptionApiView Exception { get; set; }
        public Guid UserId { get; set; }

        public SendUnexpectedExceptionRequest(UnexpectedExceptionApiView exception, Guid userId)
        {
            Exception = exception;
            UserId = userId;
        }
    }
}
