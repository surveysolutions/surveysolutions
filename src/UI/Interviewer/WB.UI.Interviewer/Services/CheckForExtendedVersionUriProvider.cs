using WB.Core.BoundedContexts.Interviewer.Services;

namespace WB.UI.Interviewer.Services
{
    public class CheckForExtendedVersionUriProvider : ICheckVersionUriProvider
    {
        public string CheckVersionUrl { get; } = "api/interviewer/extended/";
    }
}