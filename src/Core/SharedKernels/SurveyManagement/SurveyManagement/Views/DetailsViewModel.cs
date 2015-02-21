using System.Collections.Generic;
using WB.Core.SharedKernels.SurveyManagement.Views.ChangeStatus;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Views
{
    public class DetailsViewModel
    {
        public InterviewDetailsFilter Filter { get; set; }
        public InterviewDetailsView InterviewDetails { get; set; }
        public IEnumerable<InterviewGroupView> FilteredGroups { get; set; }
        public ChangeStatusView History { get; set; }
        public DetailsStatisticView Statistic { get; set; }
        public bool HasUnprocessedSyncPackages { get; set; }
    }
}
