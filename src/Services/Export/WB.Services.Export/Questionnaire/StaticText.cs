using System;
using System.Collections.Generic;

namespace WB.Services.Export.Questionnaire
{
    public class StaticText : IQuestionnaireEntity
    {
        private IQuestionnaireEntity parent;

        public StaticText()
        {
            Children = new List<IQuestionnaireEntity>();
        }

        public Guid PublicKey { get; }

        public IEnumerable<IQuestionnaireEntity> Children { get; }

        public IQuestionnaireEntity GetParent()
        {
            return parent;
        }

        public void SetParent(IQuestionnaireEntity parent)
        {
            this.parent = parent;
        }
    }
}