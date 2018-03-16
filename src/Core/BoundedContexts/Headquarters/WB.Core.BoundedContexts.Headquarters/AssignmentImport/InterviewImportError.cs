using System.Linq;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport
{
    public class InterviewImportError
    {
        public InterviewImportError(string code, string message, params InterviewImportReference[] references)
        {
            this.ErrorCode = code;
            this.ErrorMessage = message;
            this.References = references.ToArray();
        }

        public InterviewImportReference[] References { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorCode { get; set; }
    }
}