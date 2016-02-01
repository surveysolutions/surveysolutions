namespace WB.UI.Designer.Models
{
    public class VerificationResult
    {
        public VerificationResult()
        {
            this.Errors = new VerificationMessage[0];
        }

        public VerificationMessage[] Errors { get; set; }
      
        public VerificationMessage[] Warnings { get; set; }
    }
}