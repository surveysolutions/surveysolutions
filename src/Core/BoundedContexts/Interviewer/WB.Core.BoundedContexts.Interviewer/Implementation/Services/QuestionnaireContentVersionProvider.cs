using System;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class QuestionnaireContentVersionProvider : IQuestionnaireContentVersionProvider
    {
        public Version GetSupportedQuestionnaireContentVersion() => new Version(ApiVersion.MaxQuestionnaireVersion, 0, 0);
    }
}
