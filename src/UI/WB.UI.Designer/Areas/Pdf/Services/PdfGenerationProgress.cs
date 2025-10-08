using System;
using System.IO;

namespace WB.UI.Designer.Areas.Pdf.Services;

public  class PdfGenerationProgress
{
    private DateTime? finishTime;
    private DateTime? startTime;
    public PdfGenerationStatus Status { get; private set; } = PdfGenerationStatus.InQuery;

    public string FilePath { get; } = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
    public TimeSpan TimeSinceStarted => this.startTime.HasValue ? DateTime.UtcNow - this.startTime.Value : TimeSpan.Zero;
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
