using System;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class QuestionnaireContentVersionProvider : IQuestionnaireContentVersionProvider
    {
        public Version GetSupportedQuestionnaireContentVersion() => new Version(ApiVersion.MaxQuestionnaireVersion, 0, 0);
    }
}
