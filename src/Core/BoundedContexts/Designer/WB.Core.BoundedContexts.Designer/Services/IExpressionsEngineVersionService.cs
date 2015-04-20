using System;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IExpressionsEngineVersionService
    {
        Version GetLatestSupportedVersion();

        bool IsClientVersionSupported(Version clientVersion);
    }
}