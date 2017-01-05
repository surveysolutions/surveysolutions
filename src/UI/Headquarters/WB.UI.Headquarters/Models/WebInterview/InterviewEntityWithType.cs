using WB.Core.SharedKernels.DataCollection;

namespace WB.UI.Headquarters.Models.WebInterview
{
    public class InterviewEntityWithType
    {
        public InterviewEntityType EntityType { get; set; }
        public Identity Identity { get; set; }
    }
}