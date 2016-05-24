using System;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class CompleteQuestionSettings
    {
        public Guid QuestionnaireId { get; set; }
        
        public Guid ParentGroupPublicKey { get; set; }

        public Guid? PropogationPublicKey { get; set; }
    }
}