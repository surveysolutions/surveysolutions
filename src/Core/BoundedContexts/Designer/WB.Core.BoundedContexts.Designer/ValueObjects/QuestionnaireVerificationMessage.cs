using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Designer.ValueObjects
{
    public class QuestionnaireVerificationMessage
    {
        private readonly string message;

        public class CodeAndReferencesAndTranslationComparer : IEqualityComparer<QuestionnaireVerificationMessage>
        {
            public bool Equals(QuestionnaireVerificationMessage first, QuestionnaireVerificationMessage second)
                => HaveSameCodeAndReferencesAndTranslation(first, second);

            public int GetHashCode(QuestionnaireVerificationMessage message) => message.Code.GetHashCode();
        }

        public static QuestionnaireVerificationMessage Error(string code, string message, string translationName = "", params QuestionnaireVerificationReference[] references)
            => new QuestionnaireVerificationMessage(code, message, null, VerificationMessageLevel.General, translationName, references);

        public static QuestionnaireVerificationMessage Error(string code, string message,params QuestionnaireVerificationReference[] references)
            => new QuestionnaireVerificationMessage(code, message, null, VerificationMessageLevel.General, null, references);

        public static QuestionnaireVerificationMessage Error(string code, string message, IEnumerable<string> compilationErrorMessages, params QuestionnaireVerificationReference[] references)
            => new QuestionnaireVerificationMessage(code, message, compilationErrorMessages, VerificationMessageLevel.General, null, references);

        public static QuestionnaireVerificationMessage Warning(string code, string message, params QuestionnaireVerificationReference[] references)
            => new QuestionnaireVerificationMessage(code, message, null, VerificationMessageLevel.Warning, null, references);

        public static QuestionnaireVerificationMessage Critical(string code, string message, params QuestionnaireVerificationReference[] references)
            => new QuestionnaireVerificationMessage(code, message, null, VerificationMessageLevel.Critical, null, references);

        private QuestionnaireVerificationMessage(
            string code, 
            string message,
            IEnumerable<string> compilationErrorMessages,
            VerificationMessageLevel messageLevel,
            string translationName,
            IEnumerable<QuestionnaireVerificationReference> references)
        {
            this.Code = code;
            this.message = message;
            this.CompilationErrorMessages = compilationErrorMessages;
            this.MessageLevel = messageLevel;
            this.TranslationName = translationName;
            this.References = (references ?? Enumerable.Empty<QuestionnaireVerificationReference>()).ToReadOnlyCollection();
        }

        public string Code { get; }

        public string Message => string.IsNullOrWhiteSpace(this.TranslationName) 
            ? this.message :
            $"{this.TranslationName}: {this.message}";

        public IEnumerable<string> CompilationErrorMessages { get; }
        public VerificationMessageLevel MessageLevel { get; }
        public string TranslationName { get; set; }
        public IReadOnlyCollection<QuestionnaireVerificationReference> References { get; }

        public override string ToString() => string.IsNullOrWhiteSpace(this.TranslationName) 
            ? $"{this.Code}: {this.Message}"
            : $"{this.Code} ({TranslationName}): {this.Message}";

        public static bool HaveSameCodeAndReferencesAndTranslation(QuestionnaireVerificationMessage first, QuestionnaireVerificationMessage second)
            => first.Code == second.Code
            && first.References.SequenceEqual(second.References)
            && first.TranslationName == second.TranslationName;
    }
}