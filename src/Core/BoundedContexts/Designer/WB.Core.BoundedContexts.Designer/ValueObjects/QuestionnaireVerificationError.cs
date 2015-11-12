using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace WB.Core.BoundedContexts.Designer.ValueObjects
{
    public class QuestionnaireVerificationError
    {
        private readonly QuestionnaireVerificationReference[] references;

        public QuestionnaireVerificationError(string code, 
            string message,
            VerificationErrorLevel errorLevel,
            params QuestionnaireVerificationReference[] references)
        {
            this.Code = code;
            this.Message = message;
            this.ErrorLevel = errorLevel;
            this.references = references ?? new QuestionnaireVerificationReference[0];
        }

        public QuestionnaireVerificationError(string code, 
            string message,
            VerificationErrorLevel errorLevel,
            IEnumerable<QuestionnaireVerificationReference> references)
            : this(code, message, errorLevel, references.ToArray()) { }

        public string Code { get; private set; }

        public string Message { get; private set; }

        public VerificationErrorLevel ErrorLevel { get; set; }

        public IReadOnlyCollection<QuestionnaireVerificationReference> References
        {
            get { return new ReadOnlyCollection<QuestionnaireVerificationReference>(this.references); }
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", this.Code, this.Message);
        }
    }
}