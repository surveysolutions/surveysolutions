using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.SharedKernels.Questionnaire.Categories;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IReusableCategoriesService
    {
        void CloneCategories(Guid questionnaireId, Guid categoriesId, Guid clonedQuestionnaireId,
            Guid clonedCategoriesId);
        void Store(Guid questionnaireId, Guid categoriesId, Stream file, CategoriesFileType fileType);
        void Store(Guid questionnaireId, Guid categoriesId, List<CategoriesRow> categoriesRows);
        List<CategoriesRow> GetRowsFromFile(Stream file, CategoriesFileType fileType);
        byte[] GetTemplate(CategoriesFileType fileType);
        IQueryable<CategoriesItem> GetCategoriesById(Guid questionnaireId, Guid id);
        CategoriesFile? GetAsFile(QuestionnaireRevision questionnaireRevision, Guid categoriesId, CategoriesFileType fileType, bool hqImport);
        void DeleteAllByQuestionnaireId(Guid questionnaireId);
    }
}
