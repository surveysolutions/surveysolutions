using System;
using WB.Core.BoundedContexts.Interviewer.Services;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class QuestionnaireContentVersionProvider : IQuestionnaireContentVersionProvider
    {
        public Version GetSupportedQuestionnaireContentVersion() => new Version(15, 0, 0);
    }
}
