using System;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class UpdateExposedVariablesRequestModel
    {
        public string QuestionnaireIdentity { get; set; }
        public int[] Variables { get; set; }
    }
}
