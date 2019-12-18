using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Designer.Verifier
{
    public class CategoriesVerifications : AbstractVerifier, IPartialVerifier
    {
        private readonly IKeywordsProvider keywordsProvider;

        public CategoriesVerifications(IKeywordsProvider keywordsProvider)
        {
            this.keywordsProvider = keywordsProvider;
        }

        private IEnumerable<Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>>> ErrorsVerifiers => new[]
        {
            Error("WB0289", VariableNameTooLong, string.Format(VerificationMessages.WB0289, DefaultVariableLengthLimit)),
            Error("WB0290", VariableNameEndWithUnderscore, VerificationMessages.WB0290),
            Error("WB0291", VariableNameHasConsecutiveUnderscores, VerificationMessages.WB0291),
            Critical("WB0292", VariableNameIsEmpty, string.Format(VerificationMessages.WB0292)),
            Critical("WB0293", VariableNameIsKeywords, VerificationMessages.WB0293),
            Critical("WB0294", VariableNameHasSpecialCharacters, VerificationMessages.WB0294),
            Critical("WB0295", VariableNameStartWithDigitOrUnderscore, VerificationMessages.WB0295)
        };

        private static bool VariableNameIsEmpty(Categories entity) =>
            string.IsNullOrWhiteSpace(entity.Name);

        private static bool VariableNameHasConsecutiveUnderscores(Categories entity) =>
            !VariableNameIsEmpty(entity) && entity.Name.Contains("__");

        private static bool VariableNameEndWithUnderscore(Categories entity) =>
            !VariableNameIsEmpty(entity) && entity.Name[entity.Name.Length - 1] == '_';

        private static bool VariableNameTooLong(Categories entity) =>
            !VariableNameIsEmpty(entity) && (entity.Name?.Length ?? 0) > DefaultVariableLengthLimit;

        private static bool VariableNameHasSpecialCharacters(Categories entity) =>
            !VariableNameIsEmpty(entity) &&
            entity.Name.Any(c => c != '_' && !char.IsDigit(c) && !((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')));

        private static bool VariableNameStartWithDigitOrUnderscore(Categories entity) =>
            !VariableNameIsEmpty(entity) && (char.IsDigit(entity.Name[0]) || entity.Name[0] == '_');

        private bool VariableNameIsKeywords(Categories entity) => 
            !VariableNameIsEmpty(entity) && keywordsProvider.IsReservedKeyword(entity.Name);

        public IEnumerable<QuestionnaireVerificationMessage> Verify(MultiLanguageQuestionnaireDocument multiLanguageQuestionnaireDocument)
        {
            foreach (var verifier in ErrorsVerifiers)
            foreach (var error in verifier.Invoke(multiLanguageQuestionnaireDocument))
                yield return error;
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Critical(string code, Func<Categories, bool> hasError, string message) =>
            questionnaire => questionnaire.Categories.Where(entity => hasError(entity)).Select(entity =>
                QuestionnaireVerificationMessage.Critical(code, message, QuestionnaireEntityReference.CreateForCategories(entity.Id)));

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Error(string code, Func<Categories, bool> hasError, string message) =>
            questionnaire => questionnaire.Categories.Where(entity => hasError(entity)).Select(entity =>
                QuestionnaireVerificationMessage.Error(code, message, QuestionnaireEntityReference.CreateForCategories(entity.Id)));
    }
}
