namespace WB.Core.SharedKernels.SurveySolutions.Services
{
    public interface IEngineVersionService
    {
        EngineVersion GetLatestSupportedVersion();

        bool IsClientEngineVersionSupported(EngineVersion clientVersion);
    }
}