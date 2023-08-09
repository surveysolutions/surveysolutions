using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public class ImportCategoricalOptionsResult
    {
        protected ImportCategoricalOptionsResult(params string[] errors)
        {
            this.Errors = errors;
            this.Succeeded = false;
        }

        protected ImportCategoricalOptionsResult(params QuestionnaireCategoricalOption[] importedOptions)
        {
            this.ImportedOptions = importedOptions;
            this.Succeeded = true;
        }
        public bool Succeeded { get; }
        public IEnumerable<string> Errors { get; } = new List<string>();
        public IEnumerable<QuestionnaireCategoricalOption> ImportedOptions { get; } = new List<QuestionnaireCategoricalOption>();

        public static ImportCategoricalOptionsResult Success(params QuestionnaireCategoricalOption[] importedOptions)
            => new ImportCategoricalOptionsResult(importedOptions);

        public static ImportCategoricalOptionsResult Success(params CategoriesRow[] categoriesRows)
            => new ImportCategoricalOptionsResult(categoriesRows.Select(o => new QuestionnaireCategoricalOption()
            {
                Title = o.Text,
                Value = Convert.ToInt32(o.Id),
                ParentValue = string.IsNullOrWhiteSpace(o.ParentId) ? null : Convert.ToInt32(o.ParentId),
                AttachmentName = o.AttachmentName
            }).ToArray());

        public static ImportCategoricalOptionsResult Failed(params string[] errors) 
            => new ImportCategoricalOptionsResult(errors);
    }
}
