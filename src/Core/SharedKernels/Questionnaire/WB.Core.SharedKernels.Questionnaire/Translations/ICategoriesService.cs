using System;

namespace WB.Core.SharedKernels.Questionnaire.Translations
{
    public interface ICategoriesService
    {
        void CloneCategories(Guid questionnaireId, Guid categoriesId, Guid clonedQuestionnaireId,
            Guid clonedCategoriesId);
    }
}
