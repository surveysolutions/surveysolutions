using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto
{
    public class QuestionDeleted : QuestionnaireActive
    {
        public QuestionDeleted() {}

        public QuestionDeleted(Guid questionId)
        {
            this.QuestionId = questionId;
        }

        [Obsolete]
        public QuestionDeleted(Guid questionId, Guid parentPublicKey)
            : this(questionId)
        {
            this.ParentPublicKey = parentPublicKey;
        }

        public Guid QuestionId { get; set; }

        [Obsolete]
        public Guid ParentPublicKey { get; set; }
    }
}