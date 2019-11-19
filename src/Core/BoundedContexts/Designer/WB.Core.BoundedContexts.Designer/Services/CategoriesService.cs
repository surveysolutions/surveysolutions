using System;
using WB.Core.SharedKernels.Questionnaire.Categories;

namespace WB.Core.BoundedContexts.Designer.Services
{
    internal class CategoriesService : ICategoriesService
    {
        public void CloneCategories(Guid questionnaireId, Guid categoriesId, Guid clonedQuestionnaireId, Guid clonedCategoriesId)
        {
            
        }

        public CategoriesFile GetTemplateAsExcelFile()
        {
            return null;
        }

        public void Store(Guid questionnaireId, Guid categoriesId, byte[] fileBytes)
        {
            
        }
    }
}
