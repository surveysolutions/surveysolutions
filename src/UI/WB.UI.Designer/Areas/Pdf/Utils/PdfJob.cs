using System;
using System.Threading;
using System.Threading.Tasks;
using WB.UI.Designer.Areas.Pdf.Services;

namespace WB.UI.Designer.Areas.Pdf.Utils;

public class PdfJob
{
    public string Key { get; }
    public Guid UserId { get; }
    public Func<PdfGenerationProgress, CancellationToken, Task> Work { get; }
    public PdfGenerationProgress Progress { get; }
    
    public PdfJob(string key, Guid userId, Func<PdfGenerationProgress, CancellationToken, Task> work)
    {
        Key = key;
        UserId = userId;
        Progress = new PdfGenerationProgress();
        Work = work;
    }
}
