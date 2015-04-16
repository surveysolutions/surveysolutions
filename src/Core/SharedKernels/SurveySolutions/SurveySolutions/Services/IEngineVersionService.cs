namespace WB.Core.SharedKernels.SurveySolutions.Services
{
    public interface IEngineVersionService
    {
        EngineVersion GetCurrentEngineVersion();

        bool IsClientVersionSupported(EngineVersion engineVersion, EngineVersion clientVersion);
    }
}