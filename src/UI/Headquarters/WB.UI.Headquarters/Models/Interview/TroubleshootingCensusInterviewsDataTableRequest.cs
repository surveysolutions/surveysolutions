using System;
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
}