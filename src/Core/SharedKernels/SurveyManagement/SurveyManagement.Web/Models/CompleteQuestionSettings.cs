using System;

namespace WB.UI.Headquarters.Models
{
    public class CompleteQuestionSettings
    {
        public Guid QuestionnaireId { get; set; }
        
        public Guid ParentGroupPublicKey { get; set; }

        public Guid? PropogationPublicKey { get; set; }
    }
}