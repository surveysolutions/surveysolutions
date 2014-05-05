using Moq;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.SupportedVersionProviderTests
{
    internal class SupportedVersionProviderTestContext
    {
        protected static SupportedVersionProvider CreateSupportedVersionProvider(ApplicationVersionSettings settings)
        {
            return new SupportedVersionProvider(settings);
        }
    }
}
