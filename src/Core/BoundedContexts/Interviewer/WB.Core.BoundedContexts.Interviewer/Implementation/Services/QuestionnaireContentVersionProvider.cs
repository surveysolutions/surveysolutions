using WB.Core.BoundedContexts.Interviewer.Services;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class QuestionnaireContentVersionProvider : IQuestionnaireContentVersionProvider
    {
        public long GetSupportedQuestionnaireContentVersion()
        {
            return 12;
        }
    }
}
