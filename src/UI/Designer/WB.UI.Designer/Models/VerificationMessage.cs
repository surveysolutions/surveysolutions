using System.Collections.Generic;

namespace WB.UI.Designer.Models
{
    public class VerificationMessage
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public bool IsGroupedMessage { get; set; }
        public List<VerificationMessageError> Errors { get; set; }
    }

    public class VerificationMessageError
    {
        public IEnumerable<string> CompilationErrorMessages { get; set; }
        public List<VerificationReferenceEnriched> References { get; set; }
    }
}