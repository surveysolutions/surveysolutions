using System;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class DeleteQuestionnaireRequestModel
    {
        public Guid QuestionnaireId { get; set; }
        public int Version { get; set; }
    }
}