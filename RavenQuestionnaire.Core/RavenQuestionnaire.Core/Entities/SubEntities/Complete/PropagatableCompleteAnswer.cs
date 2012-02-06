using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete
{
    public class PropagatableCompleteAnswer : CompleteAnswer, IPropogate
    {
        public PropagatableCompleteAnswer()
        {
        }

      /*  public PropagatableCompleteAnswer(IAnswer answer, Guid questionPublicKey):base(answer,questionPublicKey)
        {
        }*/
        public PropagatableCompleteAnswer(ICompleteAnswer answer, Guid propogationPublicKey):base(answer,answer.QuestionPublicKey)
        {
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
