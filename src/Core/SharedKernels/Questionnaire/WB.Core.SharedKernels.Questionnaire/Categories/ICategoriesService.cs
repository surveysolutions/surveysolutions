using System;

namespace WB.Core.SharedKernels.Questionnaire.Categories
{
    public interface ICategoriesService
    {
        void CloneCategories(Guid questionnaireId, Guid categoriesId, Guid clonedQuestionnaireId,
            Guid clonedCategoriesId);

        void Store(Guid questionnaireId, Guid categoriesId, byte[] fileBytes);
        byte[] GetTemplateAsExcelFile();
        CategoriesFile GetAsExcelFile(Guid questionnaireId, Guid categoriesId);
        CategoriesFile GetPlainContentFile(Guid questionnaireId, Guid categoriesId);
    }
}
