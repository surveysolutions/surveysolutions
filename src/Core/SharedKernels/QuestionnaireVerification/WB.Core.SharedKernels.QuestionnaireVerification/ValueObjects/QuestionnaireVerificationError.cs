using System.Collections.Generic;
using System.Linq;

namespace WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects
{
    public class QuestionnaireVerificationError
    {
        public QuestionnaireVerificationError(string code, string message, params QuestionnaireVerificationReference[] references)
        {
            this.Code = code;
            this.Message = message;
            this.References = references.ToList();
        }

        public QuestionnaireVerificationError(string code, string message, IEnumerable<QuestionnaireVerificationReference> references)
            : this(code, message, references.ToArray()) { }

        public string Code { get; private set; }
        public string Message { get; private set; }
        public IEnumerable<QuestionnaireVerificationReference> References { get; private set; }

        public override string ToString()
        {
            return string.Format("{0}: {1}", this.Code, this.Message);
        }
    }
}