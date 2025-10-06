using System;
using System.Threading.Tasks;
using WB.UI.Designer.Areas.Pdf.Services;

namespace WB.UI.Designer.Areas.Pdf.Utils;

public interface IPdfQuery
{
    PdfGenerationProgress GetOrAdd(Guid userId, 
        string key,
        Func<PdfGenerationProgress, Task> runGeneration);

    void Remove(string key);
    PdfGenerationProgress? GetOrNull(string key);
    string GetQueryInfoJson();
}
