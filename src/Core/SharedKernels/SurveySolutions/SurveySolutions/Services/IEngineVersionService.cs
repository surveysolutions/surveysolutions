namespace WB.Core.SharedKernels.SurveySolutions.Services
{
    public interface IEngineVersionService
    {
        EngineVersion GetCurrentEngineVersion();

        bool IsClientEngineVersionSupported(EngineVersion clientVersion);
    }
}