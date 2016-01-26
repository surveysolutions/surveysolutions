using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace WB.Core.BoundedContexts.Designer.ValueObjects
{
    public class QuestionnaireVerificationMessage
    {
        private readonly QuestionnaireVerificationReference[] references;

        public QuestionnaireVerificationMessage(string code, 
            string message,
            VerificationMessageLevel messageLevel,
            params QuestionnaireVerificationReference[] references)
        {
            this.Code = code;
            this.Message = message;
            this.MessageLevel = messageLevel;
            this.references = references ?? new QuestionnaireVerificationReference[0];
        }

        public QuestionnaireVerificationMessage(string code, 
            string message,
            VerificationMessageLevel messageLevel,
            IEnumerable<QuestionnaireVerificationReference> references)
            : this(code, message, messageLevel, references.ToArray()) { }

        public string Code { get; private set; }

        public string Message { get; private set; }

        public VerificationMessageLevel MessageLevel { get; set; }

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