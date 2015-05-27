using System;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities
{
    public class QuestionnaireReferenceModel
    {
        public Guid Id { get; set; }
        
        public Type ModelType { get; set; }

        public bool IsRoster { get { return this.ModelType == typeof(RosterModel); } }
    }
}