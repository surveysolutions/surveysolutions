using System.Collections.Generic;
using WB.Core.SharedKernels.SurveyManagement.Views.ChangeStatus;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class DetailsViewModel
    {
        public InterviewDetailsFilter Filter { get; set; }
        public InterviewDetailsView FilteredInterviewDetails { get; set; }
        public IEnumerable<InterviewGroupView> Groups { get; set; }
        public ChangeStatusView History { get; set; }
        public DetailsStatisticView Statistic { get; set; }
    }
}
