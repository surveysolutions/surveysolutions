using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IExpressionsEngineVersionService
    {
        ExpressionsEngineVersion GetLatestSupportedVersion();

        bool IsClientVersionSupported(ExpressionsEngineVersion clientVersion);
    }
}