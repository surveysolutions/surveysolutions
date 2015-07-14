using System;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure
{
    public interface IExpressionsEngineVersionService
    {
        Version GetExpressionsEngineSupportedVersion();
    }
}
