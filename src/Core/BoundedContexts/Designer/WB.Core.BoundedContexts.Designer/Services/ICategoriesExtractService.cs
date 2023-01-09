using System.Collections.Generic;
using System.IO;
using System.Linq;
using WB.Core.SharedKernels.Questionnaire.Categories;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface ICategoriesExtractService
    {
        List<CategoriesRow> Extract(Stream file);
        byte[] GetTemplateFile();
        byte[] GetAsFile(List<CategoriesItem> items);
    }
}
