using System;
using System.IO;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface ICategoricalOptionsImportService
    {
        ImportCategoricalOptionsResult ImportOptions(Stream file, string questionnaireId, Guid categoricalQuestionId);
        ImportCategoricalOptionsResult ImportCategories(Stream file, string questionnaireId);
        Stream ExportOptions(string questionnaireId, Guid categoricalQuestionId);
    }
}
