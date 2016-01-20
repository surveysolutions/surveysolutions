using System;
using WB.Core.SharedKernels.SurveyManagement.Services;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services
{
    public class SupportedVersionProvider : ISupportedVersionProvider
    {
        private static readonly Version ExpressionsEngineVersion = new Version(11, 0, 0);

        public Version GetSupportedQuestionnaireVersion()
        {
            return ExpressionsEngineVersion;
        }
    }
}
