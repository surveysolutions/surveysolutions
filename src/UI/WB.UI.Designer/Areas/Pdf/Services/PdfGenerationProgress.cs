using System;
using System.IO;

namespace WB.UI.Designer.Areas.Pdf.Services;

public  class PdfGenerationProgress
{
    private DateTime? finishTime;

    public string FilePath { get; } = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
    public bool IsFailed { get; private set; }
    public bool IsFinished => finishTime.HasValue;
    public TimeSpan TimeSinceFinished => this.finishTime.HasValue ? DateTime.Now - this.finishTime.Value : TimeSpan.Zero;

    public void Fail() => this.IsFailed = true;
    public void Finish() => this.finishTime = DateTime.Now;
}
