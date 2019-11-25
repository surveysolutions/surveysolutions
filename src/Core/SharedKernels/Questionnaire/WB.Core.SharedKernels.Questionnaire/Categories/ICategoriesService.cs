using System;
using System.Linq;

namespace WB.Core.SharedKernels.Questionnaire.Categories
{
    public class CategoriesItem
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Text { get; set; }
    }
    public interface ICategoriesService
    {
        void CloneCategories(Guid questionnaireId, Guid categoriesId, Guid clonedQuestionnaireId,
            Guid clonedCategoriesId);

        void Store(Guid questionnaireId, Guid categoriesId, byte[] fileBytes);
        byte[] GetTemplateAsExcelFile();
        CategoriesFile GetAsExcelFile(Guid questionnaireId, Guid categoriesId);
        IQueryable<CategoriesItem> GetCategoriesById(Guid id);
        CategoriesFile GetPlainContentFile(Guid questionnaireId, Guid categoriesId);
    }
}
