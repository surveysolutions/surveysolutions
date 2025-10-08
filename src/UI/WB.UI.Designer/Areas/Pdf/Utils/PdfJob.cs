using System;
using System.Threading.Tasks;
using WB.UI.Designer.Areas.Pdf.Services;

namespace WB.UI.Designer.Areas.Pdf.Utils;

public class PdfJob
{
    public string Key { get; }
    public Guid UserId { get; }
    public Func<PdfGenerationProgress, Task> Work { get; }
    public PdfGenerationProgress Progress { get; }
    public DateTime? StartedTime { get; set; }
    
    public PdfJob(string key, Guid userId, Func<PdfGenerationProgress, Task> work)
    {
        Key = key;
        UserId = userId;
        Progress = new PdfGenerationProgress();
        Work = work;
    }
}
