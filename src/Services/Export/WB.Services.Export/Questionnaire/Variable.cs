using System;
using System.Collections.Generic;

namespace WB.Services.Export.Questionnaire
{
    public class Variable : IQuestionnaireEntity
    {
        public Variable()
        {
            this.Children = new List<IQuestionnaireEntity>();
        }

        public Guid PublicKey { get; set;  }

        public IEnumerable<IQuestionnaireEntity> Children { get; set;  }

        public IQuestionnaireEntity GetParent()
        {
            return parent;
        }

        public void SetParent(IQuestionnaireEntity parent)
        {
            this.parent = parent;
        }

        private IQuestionnaireEntity parent;

        public VariableType Type { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string Expression { get; set; }
    }

    public enum VariableType
    {
        LongInteger = 1,
        Double = 2,
        Boolean = 3,
        DateTime = 4,
        String = 5
    }
}
