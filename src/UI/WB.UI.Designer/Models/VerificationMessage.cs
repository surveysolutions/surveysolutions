using System.Collections.Generic;

namespace WB.UI.Designer.Models
{
    public class VerificationMessage
    {
        public VerificationMessage(string code, string message, bool isGroupedMessage, List<VerificationMessageError> errors)
        {
            Code = code;
            Message = message;
            IsGroupedMessage = isGroupedMessage;
            Errors = errors;
        }

        public string Code { get; set; }
        public string Message { get; set; }
        public bool IsGroupedMessage { get; set; }
        public List<VerificationMessageError> Errors { get; set; }
    }
}
