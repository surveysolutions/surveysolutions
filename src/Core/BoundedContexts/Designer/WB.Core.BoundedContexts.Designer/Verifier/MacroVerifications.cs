using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Designer.Verifier
{
    public class MacroVerifications : AbstractVerifier, IPartialVerifier
    {
        private IEnumerable<Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>>> ErrorsVerifiers => new[]
        {
            ErrorForMacro(MacroHasEmptyName, "WB0014", VerificationMessages.WB0014_MacroHasEmptyName),
            ErrorForMacro(MacroHasInvalidName, "WB0010", VerificationMessages.WB0010_MacroHasInvalidName),
            ErrorsByMacrosWithDuplicateName
        };

        private static bool MacroHasEmptyName(Macro macro, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return string.IsNullOrWhiteSpace(macro.Name);
        }

        private bool MacroHasInvalidName(Macro macro, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return !IsVariableNameValid(macro.Name);
        }

        private static IEnumerable<QuestionnaireVerificationMessage> ErrorsByMacrosWithDuplicateName(MultiLanguageQuestionnaireDocument questionnaire)
        {
            return questionnaire
                .Macros
                .Where(x => !string.IsNullOrEmpty(x.Value.Name))
                .GroupBy(x => x.Value.Name, StringComparer.InvariantCultureIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group =>
                    QuestionnaireVerificationMessage.Error(
                        "WB0020",
                        VerificationMessages.WB0020_NameForMacrosIsNotUnique,
                        group.Select(e => QuestionnaireNodeReference.CreateForMacro(e.Key)).ToArray()));
        }
        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> ErrorForMacro(
            Func<Macro, MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
        {
            return (questionnaire) => questionnaire
                .Macros
                .Where(entity => hasError(entity.Value, questionnaire))
                .Select(entity => QuestionnaireVerificationMessage.Error(code, message, QuestionnaireNodeReference.CreateForMacro(entity.Key)));
        }

        public IEnumerable<QuestionnaireVerificationMessage> Verify(MultiLanguageQuestionnaireDocument multiLanguageQuestionnaireDocument)
        {
            var verificationMessagesByQuestionnaire = new List<QuestionnaireVerificationMessage>();
            foreach (var verifier in ErrorsVerifiers)
            {
                verificationMessagesByQuestionnaire.AddRange(verifier.Invoke(multiLanguageQuestionnaireDocument));
            }
            return verificationMessagesByQuestionnaire;
        }
    }
}