using System;
using WB.Core.BoundedContexts.Tester.Services;

namespace WB.Core.BoundedContexts.Tester.Implementation.Services
{
    internal class TesterExpressionsEngineVersionService : ITesterExpressionsEngineVersionService
    {
        private readonly Version questionnaireVersion = new Version(major: 12, minor: 0, build: 0);

        public Version GetExpressionsEngineSupportedVersion()
        {
            return this.questionnaireVersion;
        }
    }
}
