using System;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    public interface IExpressionsEngineVersionService
    {
        Version GetExpressionsEngineSupportedVersion();
    }
}
