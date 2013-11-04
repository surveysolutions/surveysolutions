using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.UI.Designer.Models
{
    public class JsonVerificationResult : JsonSuccessResult
    {
        public JsonVerificationResult()
        {
            this.Errors = new QuestionnaireVerificationError[0];
        }

        public QuestionnaireVerificationError[] Errors { get; set; }
    }
}