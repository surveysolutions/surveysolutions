using System;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.UI.Headquarters.Models.Api;

namespace WB.UI.Headquarters.Models.Interview
{
    public class TroubleshootingCensusInterviewsDataTableRequest : DataTableRequest
    {
        public string QuestionnaireId { get; set; }

        public DateTime? ChangedFrom { get; set; }

        public DateTime? ChangedTo { get; set; }

        public Guid? InterviewerId { get; set; }
    }

    public class TroubleshootingCensusInterviewsDataTableResponse : DataTableResponse<InterviewListItem>
    {
        public long BrokenPackagesCount { get; set; }
        public long CensusInterviewsCount { get; set; }
    }
}