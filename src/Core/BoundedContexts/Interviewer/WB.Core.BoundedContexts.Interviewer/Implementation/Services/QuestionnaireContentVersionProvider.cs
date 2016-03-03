using System;
using WB.Core.BoundedContexts.Interviewer.Services;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class QuestionnaireContentVersionProvider : IQuestionnaireContentVersionProvider
    {
        public Version GetSupportedQuestionnaireContentVersion()
        {
            return new Version(12, 0, 0);
        }
    }
}
