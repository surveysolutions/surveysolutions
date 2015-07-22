using System;
using WB.Core.BoundedContexts.Tester.Infrastructure;
using WB.Core.BoundedContexts.Tester.Services;

namespace WB.Core.Infrastructure.Android.Implementation.Services.Settings
{
    public class ExpressionsEngineVersionService : IExpressionsEngineVersionService
    {
        private readonly Version questionnaireVersion = new Version(major: 6, minor: 0, build: 0);

        public Version GetExpressionsEngineSupportedVersion()
        {
            return this.questionnaireVersion;
        }
    }
}
