using System;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    //TODO: Remove when HQ part is separated
    [Obsolete("Remove when HQ app will be separate")]
    public interface ISupportedVersionProvider
    {
         QuestionnaireVersion GetSupportedQuestionnaireVersion();
    }
}
