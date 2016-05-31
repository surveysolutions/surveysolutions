using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Designer.ValueObjects
{
    public class QuestionnaireVerificationMessage
    {
        public class CodeAndReferencesComparer : IEqualityComparer<QuestionnaireVerificationMessage>
        {
            public bool Equals(QuestionnaireVerificationMessage first, QuestionnaireVerificationMessage second)
                => HaveSameCodeAndReferences(first, second);

            public int GetHashCode(QuestionnaireVerificationMessage message) => message.Code.GetHashCode();
        }

        public static QuestionnaireVerificationMessage Error(string code, string message, params QuestionnaireVerificationReference[] references)
            => new QuestionnaireVerificationMessage(code, message, null, VerificationMessageLevel.General, references);

        public static QuestionnaireVerificationMessage Error(string code, string message, IEnumerable<string> compilationErrorMessages, params QuestionnaireVerificationReference[] references)
            => new QuestionnaireVerificationMessage(code, message, compilationErrorMessages, VerificationMessageLevel.General, references);

        public static QuestionnaireVerificationMessage Warning(string code, string message, params QuestionnaireVerificationReference[] references)
            => new QuestionnaireVerificationMessage(code, message, null, VerificationMessageLevel.Warning, references);

        public static QuestionnaireVerificationMessage Critical(string code, string message, params QuestionnaireVerificationReference[] references)
            => new QuestionnaireVerificationMessage(code, message, null, VerificationMessageLevel.Critical, references);

        private QuestionnaireVerificationMessage(
            string code, 
            string message,
            IEnumerable<string> compilationErrorMessages,
            VerificationMessageLevel messageLevel,
            IEnumerable<QuestionnaireVerificationReference> references)
        {
            this.Code = code;
            this.Message = message;
            this.CompilationErrorMessages = compilationErrorMessages;
            this.MessageLevel = messageLevel;
            this.References = (references ?? Enumerable.Empty<QuestionnaireVerificationReference>()).ToReadOnlyCollection();
        }

        public string Code { get; }
        public string Message { get; }
        public IEnumerable<string> CompilationErrorMessages { get; }
        public VerificationMessageLevel MessageLevel { get; }
        public IReadOnlyCollection<QuestionnaireVerificationReference> References { get; }

        public override string ToString() => $"{this.Code}: {this.Message}";

        public static bool HaveSameCodeAndReferences(QuestionnaireVerificationMessage first, QuestionnaireVerificationMessage second)
            => first.Code == second.Code
            && first.References.SequenceEqual(second.References);
    }
}