using System;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models.Api
{
    public class BatchUploadModel
    {
        public string InterviewId { get; set; }
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }
        public Guid? ResponsibleSupervisor { get; set; }
    }
}
