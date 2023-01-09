using System;
using System.IO;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface ICategoricalOptionsImportService
    {
        ImportCategoricalOptionsResult ImportOptions(Stream file, string questionnaireId, Guid categoricalQuestionId, CategoriesFileType fileType);
        Stream ExportOptions(string questionnaireId, Guid categoricalQuestionId, CategoriesFileType fileType);
    }
}
