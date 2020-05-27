namespace WB.Core.SharedKernels.Questionnaire.Synchronization.Designer
{
    public class PdfStatus
    {
        public string? Message { get; set; }
        public bool ReadyForDownload { get; set; }
        public bool CanRetry { get; set; }
    }
}
