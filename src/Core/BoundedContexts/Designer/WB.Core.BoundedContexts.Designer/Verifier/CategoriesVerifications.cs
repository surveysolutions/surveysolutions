using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Designer.Verifier
{
    public class CategoriesVerifications : AbstractVerifier, IPartialVerifier
    {
        private readonly ICategoriesService categoriesService;

        public CategoriesVerifications(ICategoriesService categoriesService)
        {
            this.categoriesService = categoriesService;
        }

        private IEnumerable<Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>>> ErrorsVerifiers => new[]
        {
            Error("WB0289", CategoriesHaveMoreThanMaxOptionsCount, VerificationMessages.WB0289.FormatString(MaxOptionsCountInFilteredComboboxQuestion)),
            Error("WB0290", CategoryHasEmptyTitle, VerificationMessages.WB0290),
            Error("WB0291", CategoriesHaveDuplicatedIds, VerificationMessages.WB0291),
            Error("WB0292", CategoryHasEmptyParentId, VerificationMessages.WB0292)
        };

        private bool CategoryHasEmptyParentId(MultiLanguageQuestionnaireDocument questionnaire, Categories categories)
        {
            var categoriesWithParentIds = this.categoriesService.GetCategoriesById(categories.Id).Count(x => x.ParentId != null);

            return categoriesWithParentIds > 0 && categoriesWithParentIds < this.categoriesService.GetCategoriesById(categories.Id).Count();
        }

        private bool CategoriesHaveDuplicatedIds(MultiLanguageQuestionnaireDocument questionnaire, Categories categories) =>
            this.categoriesService.GetCategoriesById(categories.Id).GroupBy(x => new {x.Id, x.ParentId}).Any(x => x.Count() > 1); 

        private bool CategoryHasEmptyTitle(MultiLanguageQuestionnaireDocument questionnaire, Categories categories) => 
            this.categoriesService.GetCategoriesById(categories.Id).Any(x => string.IsNullOrEmpty(x.Text));

        private bool CategoriesHaveMoreThanMaxOptionsCount(MultiLanguageQuestionnaireDocument questionnaire, Categories categories) =>
            this.categoriesService.GetCategoriesById(categories.Id).Count() > MaxOptionsCountInFilteredComboboxQuestion;

        public IEnumerable<QuestionnaireVerificationMessage> Verify(MultiLanguageQuestionnaireDocument multiLanguageQuestionnaireDocument) =>
            this.ErrorsVerifiers.SelectMany(verifier => verifier.Invoke(multiLanguageQuestionnaireDocument));

        protected static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Error(string code, Func<MultiLanguageQuestionnaireDocument, Categories, bool> hasError, string message) =>
            questionnaire => questionnaire.Categories.Where(entity => hasError(questionnaire, entity))
                .Select(entity => QuestionnaireVerificationMessage.Error(code, message, QuestionnaireEntityReference.CreateForCategories(entity.Id)));
    }
}
