using System;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    public class SupportedVersionProvider : ISupportedVersionProvider
    {
        public Version GetSupportedQuestionnaireVersion() => new Version(ApiVersion.QuestionnaireContent, 0, 0);
    }
}
