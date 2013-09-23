using System;

namespace WB.Core.BoundedContexts.Supervisor.Views.Questionnaire
{
    public class ReferenceInfoByQuestion
    {
        public ReferenceInfoByQuestion(Guid scopeId, Guid referencedQuestionId)
        {
            this.ScopeId = scopeId;
            this.ReferencedQuestionId = referencedQuestionId;
        }

        public Guid ScopeId { get; private set; }
        public Guid ReferencedQuestionId { get; private set; }
    }
}