using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.UI.Supervisor.Services.Implementation
{
    public class CheckForExtendedVersionUriProvider : ICheckVersionUriProvider
    {
        public string CheckVersionUrl { get; } = "api/supervisor/v1/extended/";
    }
}
