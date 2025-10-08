using System;
using System.IO;

namespace WB.UI.Designer.Areas.Pdf.Services;

public  class PdfGenerationProgress
{
    private DateTime? finishTime;
    private DateTime? startTime;
    public PdfGenerationStatus Status { get; private set; } = PdfGenerationStatus.InQueue;

    public string FilePath { get; } = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
    public DateTime? StartedTime => this.startTime;
    public TimeSpan TimeSinceFinished => this.finishTime.HasValue ? DateTime.UtcNow - this.finishTime.Value : TimeSpan.Zero;

    public void Started()
    {
        Status = PdfGenerationStatus.Started;
        startTime = DateTime.UtcNow;
    }

    public void Fail() => Status = PdfGenerationStatus.Failed;
    public void Finish()
    {
        Status = PdfGenerationStatus.Finished;
        finishTime = DateTime.UtcNow;
    }
}
