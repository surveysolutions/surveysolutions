using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.UI.Interviewer.Services
{
    public class CheckForExtendedVersionUriProvider : ICheckVersionUriProvider
    {
        public string CheckVersionUrl { get; } = "api/interviewer/extended/";
    }
}
