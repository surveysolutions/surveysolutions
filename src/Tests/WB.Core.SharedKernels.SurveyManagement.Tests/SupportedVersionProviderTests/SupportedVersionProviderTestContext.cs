using System;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.SupportedVersionProviderTests
{
    internal class SupportedVersionProviderTestContext
    {
        protected static SupportedVersionProvider CreateSupportedVersionProvider(ApplicationVersionSettings settings,
            bool? isDebug = null, int? applicationVersion = null)
        {
            return new SupportedVersionProvider(settings, () => isDebug ?? false,
                applicationVersion ?? 4);
        }
    }
}
