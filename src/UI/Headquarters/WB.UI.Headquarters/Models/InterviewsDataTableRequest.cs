using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Headquarters.Models.Api;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class InterviewsDataTableRequest : DataTableRequest
    {
        public string QuestionnaireId { get; set; }
        public long? QuestionnaireVersion { get; set; }
        public InterviewStatus[] Statuses { get; set; }
        public string SearchBy { get; set; }
        public int? AssignmentId { get; set; }
    }

    public class InterviewsDataTableResponse : DataTableResponse<AllInterviewsViewItem>
    {
    }
}