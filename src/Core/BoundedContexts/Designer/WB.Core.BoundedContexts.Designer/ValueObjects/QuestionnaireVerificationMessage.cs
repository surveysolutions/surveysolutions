using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace WB.Core.BoundedContexts.Designer.ValueObjects
{
    public class QuestionnaireVerificationMessage
    {
        private readonly QuestionnaireVerificationReference[] references;

        public static QuestionnaireVerificationMessage Error(string code,
            string message,
            params QuestionnaireVerificationReference[] references)
        {
            return new QuestionnaireVerificationMessage(code, message, VerificationMessageLevel.General, references);
        }

        public static QuestionnaireVerificationMessage Warning(string code,
            string message,
            params QuestionnaireVerificationReference[] references)
        {
            return new QuestionnaireVerificationMessage(code, message, VerificationMessageLevel.Warning, references);
        }

        public static QuestionnaireVerificationMessage Critical(string code,
            string message,
            params QuestionnaireVerificationReference[] references)
        {
            return new QuestionnaireVerificationMessage(code, message, VerificationMessageLevel.Critical, references);
        }

        private QuestionnaireVerificationMessage(string code, 
            string message,
            VerificationMessageLevel messageLevel,
            params QuestionnaireVerificationReference[] references)
        {
            this.Code = code;
            this.Message = message;
            this.MessageLevel = messageLevel;
            this.references = references ?? new QuestionnaireVerificationReference[0];
        }

        public string Code { get; private set; }

        public string Message { get; private set; }

        public VerificationMessageLevel MessageLevel { get; set; }

        public IReadOnlyCollection<QuestionnaireVerificationReference> References => new ReadOnlyCollection<QuestionnaireVerificationReference>(this.references);

        public override string ToString()
        {
            return $"{this.Code}: {this.Message}";
        }
    }
}