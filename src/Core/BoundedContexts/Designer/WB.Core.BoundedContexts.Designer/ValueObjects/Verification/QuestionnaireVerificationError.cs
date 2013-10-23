using System.Collections.Generic;
using System.Linq;

namespace WB.Core.BoundedContexts.Designer.ValueObjects.Verification
{
    public class QuestionnaireVerificationError
    {
        public QuestionnaireVerificationError(string code, string message, IEnumerable<QuestionnaireVerificationReference> references)
        {
            this.Code = code;
            this.Message = message;
            this.References = references.ToList();
        }

        public string Code { get; private set; }
        public string Message { get; private set; }
        public IEnumerable<QuestionnaireVerificationReference> References { get; private set; }
    }
}