using System;

namespace Web.CAPI.Models
{
    public class CompleteQuestionSettings
    {
        public string QuestionnaireId { get; set; }
        public Guid ParentGroupPublicKey { get; set; }
        public Guid? PropogationPublicKey { get; set; }
    }

    public class QuestionRenderOptions
    {
        public bool isHorizontal { get; set;}
    }
}