using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.UI.Designer.Models
{
    public class VerificationResult : JsonSuccessResult
    {
        public VerificationResult()
        {
            this.Errors = new QuestionnaireVerificationError[0];
        }

        public QuestionnaireVerificationError[] Errors { get; set; }
    }
}