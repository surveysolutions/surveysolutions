using System;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IDesignerExpressionsEngineVersionService
    {
        Version GetLatestSupportedVersion();

        bool IsClientVersionSupported(Version clientVersion);
    }
}