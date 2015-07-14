using System;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities
{
    public class GroupReferenceModel
    {
        public Guid Id { get; set; }

        public bool IsRoster { get; set; }
    }

    public class QuestionnaireReferenceModel
    {
        public Guid Id { get; set; }
        
        public Type ModelType { get; set; }
    }
}