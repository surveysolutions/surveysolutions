using System;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class UpdateExposedVariablesRequestModel
    {
        public Guid QuestionnaireId { get; set; }
        public int Version { get; set; }
        public int[] Variables { get; set; }
    }
}
