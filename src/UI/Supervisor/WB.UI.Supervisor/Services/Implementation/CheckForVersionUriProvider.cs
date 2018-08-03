using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.UI.Supervisor.Services.Implementation
{
    public class CheckForVersionUriProvider : ICheckVersionUriProvider
    {
        public string CheckVersionUrl { get; } = "api/supervisor/";
    }
}
