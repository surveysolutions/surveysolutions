﻿using System.Collections.Generic;
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
        public IEnumerable<string> Errors { get; }
        public IEnumerable<QuestionnaireCategoricalOption> ImportedOptions { get; }

        public static ImportCategoricalOptionsResult Success(params QuestionnaireCategoricalOption[] importedOptions)
            => new ImportCategoricalOptionsResult(importedOptions);

        public static ImportCategoricalOptionsResult Failed(params string[] errors) 
            => new ImportCategoricalOptionsResult(errors);
    }
}
