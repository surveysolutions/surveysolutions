using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class DetailsViewModel
    {
        public InterviewDetailsSortBy SortBy { get; set; }
        public InterviewDetailsFilter Filter { get; set; }
        public InterviewDetailsView InterviewDetails { get; set; }
    }
}
