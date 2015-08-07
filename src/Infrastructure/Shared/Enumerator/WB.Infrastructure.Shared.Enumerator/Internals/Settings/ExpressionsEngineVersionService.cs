using System;
using WB.Core.BoundedContexts.Tester.Infrastructure;

namespace WB.Infrastructure.Shared.Enumerator.Internals.Settings
{
    internal class ExpressionsEngineVersionService : IExpressionsEngineVersionService
    {
        private readonly Version questionnaireVersion = new Version(major: 6, minor: 0, build: 0);

        public Version GetExpressionsEngineSupportedVersion()
        {
            return this.questionnaireVersion;
        }
    }
}
