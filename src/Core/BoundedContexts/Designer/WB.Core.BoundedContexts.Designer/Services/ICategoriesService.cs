using System;
using System.IO;
using System.Linq;
using WB.Core.SharedKernels.Questionnaire.Categories;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface ICategoriesService
    {
        void CloneCategories(Guid questionnaireId, Guid categoriesId, Guid clonedQuestionnaireId,
            Guid clonedCategoriesId);

        void Store(Guid questionnaireId, Guid categoriesId, Stream file, CategoriesFileType fileType);
        byte[] GetTemplateAsExcelFile();
        IQueryable<CategoriesItem> GetCategoriesById(Guid questionnaireId, Guid id);
        CategoriesFile? GetAsExcelFile(Guid questionnaireId, Guid categoriesId);
        void DeleteAllByQuestionnaireId(Guid questionnaireId);
    }
}
