using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Designer.ValueObjects
{
    public class QuestionnaireVerificationMessage
    {
        public static QuestionnaireVerificationMessage Error(string code, string message, params QuestionnaireVerificationReference[] references)
            => new QuestionnaireVerificationMessage(code, message, VerificationMessageLevel.General, references);

        public static QuestionnaireVerificationMessage Warning(string code, string message, params QuestionnaireVerificationReference[] references)
            => new QuestionnaireVerificationMessage(code, message, VerificationMessageLevel.Warning, references);

        public static QuestionnaireVerificationMessage Critical(string code, string message, params QuestionnaireVerificationReference[] references)
            => new QuestionnaireVerificationMessage(code, message, VerificationMessageLevel.Critical, references);

        private QuestionnaireVerificationMessage(
            string code, 
            string message,
            VerificationMessageLevel messageLevel,
            IEnumerable<QuestionnaireVerificationReference> references)
        {
            this.Code = code;
            this.Message = message;
            this.MessageLevel = messageLevel;
            this.References = (references ?? Enumerable.Empty<QuestionnaireVerificationReference>()).ToReadOnlyCollection();
        }

        public string Code { get; }
        public string Message { get; }
        public VerificationMessageLevel MessageLevel { get; }
        public IReadOnlyCollection<QuestionnaireVerificationReference> References { get; }

        public override string ToString() => $"{this.Code}: {this.Message}";
    }
}