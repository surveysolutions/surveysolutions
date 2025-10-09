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

    public void Start()
    {
        if (Status != PdfGenerationStatus.InQueue)
            throw new InvalidOperationException("Cannot start a job that is not in queue");
        
        Status = PdfGenerationStatus.Started;
        startTime = DateTime.UtcNow;
    }

    public void Fail()
    {
        if (Status != PdfGenerationStatus.Started)
            throw new InvalidOperationException("Cannot start a job that is not started");

        Status = PdfGenerationStatus.Failed;
    }

    public void Finish()
    {
        if (Status != PdfGenerationStatus.Started)
            throw new InvalidOperationException("Cannot start a job that is not started");

        Status = PdfGenerationStatus.Finished;
        finishTime = DateTime.UtcNow;
    }
}
