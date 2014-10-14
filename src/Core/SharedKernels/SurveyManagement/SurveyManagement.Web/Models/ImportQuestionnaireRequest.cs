using System;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class ImportQuestionnaireRequest
    {
        public Guid QuestionnaireId { get; set; }
        public bool AllowCensusMode { get; set; }
    }
}