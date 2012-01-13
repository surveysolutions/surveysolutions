using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete
{
    public class PropagatableCompleteAnswer : CompleteAnswer, IPropogate
    {

        public PropagatableCompleteAnswer(IAnswer answer, Guid questionPublicKey):base(answer,questionPublicKey)
        {
        }
        public PropagatableCompleteAnswer(CompleteAnswer answer, Guid propogationPublicKey)
        {
            this.AnswerText = answer.AnswerText;
            this.AnswerType = answer.AnswerType;
            this.CustomAnswer = answer.CustomAnswer;
            this.Mandatory = answer.Mandatory;
            this.PublicKey = answer.PublicKey;
            this.Selected = answer.Selected;

            this.PropogationPublicKey = propogationPublicKey;
        }
        #region Implementation of ICloneable

        public object Clone()
        {
            throw new InvalidOperationException("answer can't propagated");
        }

        #endregion

        #region Implementation of IPropogate

        public Guid PropogationPublicKey { get; set; }

        #endregion
    }
}
