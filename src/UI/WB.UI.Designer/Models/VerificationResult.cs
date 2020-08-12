namespace WB.UI.Designer.Models
{
    public class VerificationResult
    {
        public VerificationResult(VerificationMessage[] errors, VerificationMessage[] warnings)
        {
            Errors = errors;
            Warnings = warnings;
        }

        public VerificationMessage[] Errors { get; set; }
      
        public VerificationMessage[] Warnings { get; set; }
    }
}
