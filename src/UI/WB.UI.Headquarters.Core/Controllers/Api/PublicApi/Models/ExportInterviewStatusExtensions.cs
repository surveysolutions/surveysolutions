using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Models
{
    public static class ExportInterviewStatusExtensions
    {
        public static InterviewStatus? ToInterviewStatus(this ExportInterviewStatus? status)
        {
            return status != null ? (InterviewStatus?) status : null;
        }
    }
}
