namespace WB.UI.Designer.Areas.Pdf.Services;

public class PdfStatus
{
    private PdfStatus(string message, bool readyForDownload = false, bool canRetry = false)
    {
        this.Message = message;
        this.ReadyForDownload = readyForDownload;
        this.CanRetry = canRetry;
    }

    public string Message { get; }
    public bool ReadyForDownload { get; }
    public bool CanRetry { get; }

    public static PdfStatus InProgress(string message) => new PdfStatus(message);
    public static PdfStatus Failed(string message) => new PdfStatus(message, canRetry: true);
    public static PdfStatus Ready(string message) => new PdfStatus(message, readyForDownload: true);
}
