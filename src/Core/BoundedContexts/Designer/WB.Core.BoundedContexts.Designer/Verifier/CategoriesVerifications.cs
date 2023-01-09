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
        private readonly IReusableCategoriesService reusableCategoriesService;

        public CategoriesVerifications(IKeywordsProvider keywordsProvider, IReusableCategoriesService reusableCategoriesService)
        {
            this.keywordsProvider = keywordsProvider;
            this.reusableCategoriesService = reusableCategoriesService;
        }

        private IEnumerable<Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>>> ErrorsVerifiers => new[]
        {
            Error("WB0289", VariableNameTooLong, string.Format(VerificationMessages.WB0289, DefaultVariableLengthLimit)),
            Error("WB0290", VariableNameEndWithUnderscore, VerificationMessages.WB0290),
            Error("WB0291", VariableNameHasConsecutiveUnderscores, VerificationMessages.WB0291),
            Error("WB0305", HasDuplicatedPair_Id_ParentId, VerificationMessages.WB0305_DuplicatedCategoryIdParentIdPair),
            Error("WB0306", HasDuplicatedPair_ParentId_Text, VerificationMessages.WB0306_DuplicatedCategoryParentIdText),
            Critical("WB0292", VariableNameIsEmpty, string.Format(VerificationMessages.WB0292)),
            Critical("WB0293", VariableNameIsKeywords, VerificationMessages.WB0293),
            Critical("WB0294", VariableNameHasSpecialCharacters, VerificationMessages.WB0294),
            Critical("WB0295", VariableNameStartWithDigitOrUnderscore, VerificationMessages.WB0295),
            Critical("WB0312", MustHaveTwoOptionsMinimum, VerificationMessages.WB0312)
        };

        private bool HasDuplicatedPair_Id_ParentId(Categories category, MultiLanguageQuestionnaireDocument questionnaire)
        {
            var duplicated = 
                from row in this.reusableCategoriesService.GetCategoriesById(questionnaire.PublicKey, category.Id)
                group row by new { row.Id, row.ParentId}
                into g
                where g.Count() > 1
                select g.Key;

            if (duplicated.Any()) return true;

            return false;
        }     
        
        private bool HasDuplicatedPair_ParentId_Text(Categories category, MultiLanguageQuestionnaireDocument questionnaire)
        {
            var duplicated = 
                from row in this.reusableCategoriesService.GetCategoriesById(questionnaire.PublicKey, category.Id)
                group row by new { row.Text, row.ParentId}
                into g
                where g.Count() > 1
                select g.Key;

            if (duplicated.Any()) return true;

            return false;
        } 
        
        private bool MustHaveTwoOptionsMinimum(Categories category, MultiLanguageQuestionnaireDocument questionnaire)
        {
            var items = this.reusableCategoriesService.GetCategoriesById(questionnaire.PublicKey, category.Id);
            return items.Count() < 2;
        }

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
            return ErrorsVerifiers.SelectMany(verifier 
                => verifier.Invoke(multiLanguageQuestionnaireDocument));
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Critical(string code, Func<Categories, bool> hasError, string message) =>
            questionnaire => 
                questionnaire.Categories.Where(entity => hasError(entity)).Select(entity =>
                QuestionnaireVerificationMessage.Critical(code, message, QuestionnaireEntityReference.CreateForCategories(entity.Id)));

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> 
            Critical(string code, Func<Categories, MultiLanguageQuestionnaireDocument, bool> hasError, string message) =>
            questionnaire => questionnaire.Categories.Where(entity => hasError(entity, questionnaire)).Select(entity =>
                QuestionnaireVerificationMessage.Critical(code, message, QuestionnaireEntityReference.CreateForCategories(entity.Id)));

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Error(string code, Func<Categories, bool> hasError, string message) =>
            questionnaire => questionnaire.Categories.Where(entity => hasError(entity)).Select(entity =>
                QuestionnaireVerificationMessage.Error(code, message, QuestionnaireEntityReference.CreateForCategories(entity.Id)));

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> 
            Error(string code, Func<Categories, MultiLanguageQuestionnaireDocument, bool> hasError, string message) =>
            questionnaire => questionnaire.Categories.Where(entity => hasError(entity, questionnaire)).Select(entity =>
                QuestionnaireVerificationMessage.Error(code, message, QuestionnaireEntityReference.CreateForCategories(entity.Id)));
    }
}
