namespace WB.UI.Designer.Models
{
    public class VerificationResult
    {
        public VerificationResult()
        {
            this.Errors = new VerificationMessage[0];
        }

        public VerificationMessage[] Errors { get; set; }
        public int ErrorsCount { get; set; }

        public VerificationMessage[] Warnings { get; set; }
        public int WarningsCount { get; set; }
    }
}