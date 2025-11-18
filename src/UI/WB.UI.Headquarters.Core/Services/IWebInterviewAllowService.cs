using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.UI.Headquarters.API.WebInterview
{
    public interface IWebInterviewAllowService
    {
        void CheckWebInterviewAccessPermissions(string interviewId);
    }
}
