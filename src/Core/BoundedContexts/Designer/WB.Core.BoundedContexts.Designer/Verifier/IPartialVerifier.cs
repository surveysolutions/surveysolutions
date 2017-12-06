using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Core.BoundedContexts.Designer.Verifier
{
    public interface IPartialVerifier
    {
        IEnumerable<QuestionnaireVerificationMessage> Verify(
            MultiLanguageQuestionnaireDocument multiLanguageQuestionnaireDocument);
    }
}