using System;
using System.Collections.Generic;

namespace WB.Services.Export.Questionnaire
{
    public abstract class Question : IQuestionnaireEntity
    {
        private IQuestionnaireEntity parent;

        public Question()
        {
            Answers = new List<Answer>();
            Children = new List<IQuestionnaireEntity>();
        }

        public QuestionType QuestionType { get; set; }

        public bool Featured { get; set;  }

        public string VariableName { get; set;  }

        public string VariableLabel { get; set;  }

        public string QuestionText { get; set; }

        public List<Answer> Answers { get; set; }

        public Guid? LinkedToQuestionId { get; set; }

        public Guid? LinkedToRosterId { get; set; }

        public Guid PublicKey { get; set; }

        public IEnumerable<IQuestionnaireEntity> Children { get; }

        public IQuestionnaireEntity GetParent()
        {
            return parent;
        }

        public void SetParent(IQuestionnaireEntity parent)
        {
            this.parent = parent;
        }

        public bool IsQuestionLinked()
        {
            return LinkedToQuestionId != null || LinkedToRosterId != null;
        }
    }
}
