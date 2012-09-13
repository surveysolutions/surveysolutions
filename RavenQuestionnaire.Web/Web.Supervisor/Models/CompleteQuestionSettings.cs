using System;

namespace Web.Supervisor.Models
{
    public class CompleteQuestionSettings
    {
        public Guid QuestionnaireId { get; set; }
        public Guid ParentGroupPublicKey { get; set; }
        public Guid? PropogationPublicKey { get; set; }
    }
/*
    public class QuestionRenderOptions
    {
        public bool isHorizontal { get; set;}
    }*/
}