
namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class QuestionnaireVerificationResponse
    {
        public QuestionnaireVerificationResponse()
        {
            IsSuccess = true;
        }
        public bool IsSuccess { get; set; }
        public string QuestionnaireTitle { get; set; }
        public string ImportError { get; set; }
    }
}