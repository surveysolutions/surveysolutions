namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier
{
    public class AssignmentVerificationResult
    {
        public bool Status { get; }
        public string ErrorMessage { get; }

        private AssignmentVerificationResult(bool status, string errorMessage)
        {
            Status = status;
            ErrorMessage = errorMessage;
        }

        public static AssignmentVerificationResult Error(string message)
        {
            return new AssignmentVerificationResult(false, message);
        }

        public static AssignmentVerificationResult Ok()
        {
            return new AssignmentVerificationResult(true, null);
        }
    }
}
