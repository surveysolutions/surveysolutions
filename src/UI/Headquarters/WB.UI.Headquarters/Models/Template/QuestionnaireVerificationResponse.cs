using System.Linq;
using Main.Core.Documents;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.UI.Headquarters.Models.Template
{
    public class QuestionnaireVerificationResponse
    {
        public QuestionnaireVerificationResponse(bool isSuccess, string questionnaireTitle = null)
        {
            this.IsSuccess = isSuccess;
            this.QuestionnaireTitle = questionnaireTitle;
        }

        public void SetErrorsForQuestionnaire(QuestionnaireVerificationError[] errors, QuestionnaireDocument questionnaire)
        {
            this.Errors = errors.Select(error => new QuestionnaireVerificationErrorResponse(error, questionnaire)).ToArray();
        }

        public bool IsSuccess { get; private set; }
        public string QuestionnaireTitle { get; private set; }
        public QuestionnaireVerificationErrorResponse[] Errors { get; private set; }
        public string ImportError { get; set; }
    }
}