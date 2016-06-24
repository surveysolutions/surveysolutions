using System;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    public class SupportedVersionProvider : ISupportedVersionProvider
    {
        public int GetSupportedQuestionnaireVersion() => ApiVersion.QuestionnaireContent;
    }
}
