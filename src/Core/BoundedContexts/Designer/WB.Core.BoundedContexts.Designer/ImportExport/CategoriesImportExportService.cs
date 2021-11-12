using System;
using System.Linq;
using System.Runtime.InteropServices;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Commands;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Categories;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.ImportExport.Models;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Translations;

namespace WB.Core.BoundedContexts.Designer.ImportExport
{
    public class CategoriesImportExportService : ICategoriesImportExportService
    {
        private readonly DesignerDbContext dbContext;
        private readonly IQuestionnaireSerializer questionnaireSerializer;

        public CategoriesImportExportService(DesignerDbContext dbContext,
            IQuestionnaireSerializer questionnaireSerializer)
        {
            this.dbContext = dbContext;
            this.questionnaireSerializer = questionnaireSerializer;
        }

        public string GetCategoriesJson(QuestionnaireDocument questionnaire, Guid categoriesId)
        {
            var questionnaireId = questionnaire.PublicKey;
            var storedCategories = dbContext.CategoriesInstances
                .Where(x => x.QuestionnaireId == questionnaireId && x.CategoriesId == categoriesId)
                .OrderBy(x => x.SortIndex)
                .ToList();

            var items = storedCategories.Select(t =>
                new CategoriesItem()
                {
                    Value = t.Value,
                    Text = t.Text,
                    ParentId = t.ParentId
                }).ToList();
            var json = questionnaireSerializer.Serialize(items);
            return json;
        }

        public void StoreCategoriesFromJson(QuestionnaireDocument questionnaire, Guid categoriesId, string json)
        {
            if (questionnaire == null) throw new ArgumentNullException(nameof(questionnaire));
            if (categoriesId == null) throw new ArgumentNullException(nameof(categoriesId));
            if (json == null) throw new ArgumentNullException(nameof(json));

            try
            {
                var items = questionnaireSerializer.Deserialize<CategoriesItem>(json);
                
                this.dbContext.CategoriesInstances.AddRange(items.Select((item, i) => new CategoriesInstance
                {
                    SortIndex = i,
                    Text = CommandUtils.SanitizeHtml(item.Text, true),
                    Value = item.Value,
                    ParentId = item.ParentId,
                    CategoriesId = categoriesId,
                    QuestionnaireId = questionnaire.PublicKey,
                }));
            }
            catch (COMException e)
            {
                throw new InvalidFileException(ExceptionMessages.CategoriesCantBeExtracted, e);
            }
        }
    }

    public interface ICategoriesImportExportService
    {
        string GetCategoriesJson(QuestionnaireDocument questionnaire, Guid categoriesId);
        void StoreCategoriesFromJson(QuestionnaireDocument questionnaire, Guid categoriesId, string json);
    }
}