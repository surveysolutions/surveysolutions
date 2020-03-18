using System;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Headquarters.Models.Api;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class InterviewsDataTableRequest : DataTableRequest
    {
        public Guid? QuestionnaireId { get; set; }
        public long? QuestionnaireVersion { get; set; }
        
        [FromQuery(Name = "statuses[]")]
        public InterviewStatus[] Statuses { get; set; }
        public string SearchBy { get; set; }
        public int? AssignmentId { get; set; }

        public DateTime? UnactiveDateStart { get; set; }
        public DateTime? UnactiveDateEnd { get; set; }

        public Guid? TeamId { get; set; }

        public Guid? ResponsibleId { get; set; }
        public string ResponsibleName { get; set; }

        public InterviewStatus? Status { get; set; }
    }

    public class InterviewsDataTableResponse : DataTableResponse<AllInterviewsViewItem>
    {
    }
    public class TeamInterviewsDataTableResponse : DataTableResponse<TeamInterviewsViewItem>
    {
    }
}
