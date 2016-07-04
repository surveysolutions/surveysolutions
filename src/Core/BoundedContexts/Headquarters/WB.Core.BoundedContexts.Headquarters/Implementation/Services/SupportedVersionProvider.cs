using System;
using WB.Core.BoundedContexts.Headquarters.Services;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    public class SupportedVersionProvider : ISupportedVersionProvider
    {
        public Version GetSupportedQuestionnaireVersion() => new Version(16, 0, 0);
    }
}
