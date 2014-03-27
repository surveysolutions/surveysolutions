using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.UI.Headquarters.Models.Template
{
    public class QuestionnaireVerificationErrorResponse
    {
        public QuestionnaireVerificationErrorResponse(QuestionnaireVerificationError error, QuestionnaireDocument questionnaire)
        {
            this.Code = error.Code;
            this.Message = error.Message;
            this.References = error.References.Select(reference => new QuestionnaireVerificationReferenceResponse(reference, questionnaire));
        }
        public string Code { get; private set; }
        public string Message { get; private set; }
        public IEnumerable<QuestionnaireVerificationReferenceResponse> References { get; private set; }
    }
}