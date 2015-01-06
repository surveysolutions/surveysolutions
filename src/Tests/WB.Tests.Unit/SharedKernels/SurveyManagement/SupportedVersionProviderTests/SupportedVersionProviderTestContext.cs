using System;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SupportedVersionProviderTests
{
    internal class SupportedVersionProviderTestContext
    {
        protected static SupportedVersionProvider CreateSupportedVersionProvider(ApplicationVersionSettings settings,
            bool? isDebug = null, Version applicationVersion = null)
        {
            return new SupportedVersionProvider(settings, () => isDebug ?? false, new Version(1, 2, 3, 4));
        }
    }
}
