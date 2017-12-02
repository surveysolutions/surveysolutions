using WB.Core.SharedKernels.DataCollection;

namespace WB.UI.Headquarters.Models.WebInterview
{
    public class InterviewEntityWithType
    {
        public string EntityType { get; set; }
        public string Identity { get; set; }
    }

    public class InterviewSectionDetails
    {
        public InterviewEntityWithType[] Entities { get; set; }
        public InterviewEntity[] Details { get; set; }
    }
}