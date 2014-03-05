using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.Core.BoundedContexts.Supervisor.Services
{
    public interface ISupportedVersionProvider
    {
         QuestionnaireVersion GetMaximalQuestionnaireVersion();
    }
}
