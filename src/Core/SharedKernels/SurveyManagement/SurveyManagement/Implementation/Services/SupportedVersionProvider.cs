using System;
using WB.Core.SharedKernels.SurveyManagement.Services;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services
{
    public class SupportedVersionProvider : ISupportedVersionProvider
    {
        public Version GetSupportedQuestionnaireVersion() => new Version(15, 0, 0);
    }
}
