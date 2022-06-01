using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IQuestionnaireVerifier
    {
        IEnumerable<QuestionnaireVerificationMessage> GetAllErrors(QuestionnaireView questionnaireView, 
            bool includeWarnings = false);
        IEnumerable<QuestionnaireVerificationMessage> CompileAndVerify(QuestionnaireView questionnaireView, 
            int? version, 
            out string assembly);
        IEnumerable<QuestionnaireVerificationMessage> CompileAndVerify(QuestionnaireView questionnaireView, 
            int? version, 
            Guid? newQuestionnaireId,
            out string assembly);
    }
}
